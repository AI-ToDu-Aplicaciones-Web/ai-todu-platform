"""Integration checks against a running API and a disposable PostgreSQL database.

Set WAREHOUSE_TEST_SIGNING_KEY to the API's test signing key. Never use a production key.
"""
import base64, concurrent.futures, hashlib, hmac, json, os, time, urllib.request, urllib.error, uuid

BASE=os.environ.get('WAREHOUSE_API_URL','http://127.0.0.1:5080')
KEY=os.environ['WAREHOUSE_TEST_SIGNING_KEY'].encode()
ORG=str(uuid.uuid4()); ACTOR=str(uuid.uuid4())
def encoded(value): return base64.urlsafe_b64encode(json.dumps(value,separators=(',',':')).encode()).rstrip(b'=')
def token(org=ORG,role='WAREHOUSE_OPERATOR',expired=False):
    payload={'sub':ACTOR,'organization_id':org,'role':role,'iss':'ai-todu','aud':'ai-todu-webapp','exp':int(time.time())+(-3600 if expired else 3600)}
    body=encoded({'alg':'HS256','typ':'JWT'})+b'.'+encoded(payload)
    return (body+b'.'+base64.urlsafe_b64encode(hmac.new(KEY,body,hashlib.sha256).digest()).rstrip(b'=')).decode()
TOKEN=token()
def request(method,path,data=None,bearer=TOKEN,language='en-US'):
    headers={'Content-Type':'application/json','Accept-Language':language}
    if bearer is not None: headers['Authorization']='Bearer '+bearer
    req=urllib.request.Request(BASE+path,data=None if data is None else json.dumps(data).encode(),headers=headers,method=method)
    try:
        with urllib.request.urlopen(req,timeout=20) as r:return r.status,json.loads(r.read() or b'null')
    except urllib.error.HTTPError as e:return e.code,json.loads(e.read() or b'null')
def expect(status,result,label):
    assert result[0]==status,(label,result)
    print('PASS',label)
    return result[1]
def mutation(item,**values):return {'operationId':str(uuid.uuid4()),'expectedVersion':item['version'],**values}
def create(sku=None,bearer=TOKEN):
    return expect(201,request('POST','/api/v1/inventory-items',{'sku':sku or 'API-'+uuid.uuid4().hex[:8],'name':'Integration rice','category':'Food','minimumStock':5,'unitPrice':10},bearer),'create product')
def receive(item,quantity=20):
    return expect(200,request('POST',f"/api/v1/inventory-items/{item['id']}/lots",mutation(item,lotCode='LOT-1',location='A-1',expiresOn='2099-12-31',quantity=quantity)),'persist stock lot')

def main():
    expect(401,request('GET','/api/v1/inventory-items',bearer='invalid'),'reject invalid bearer token')
    expect(401,request('GET','/api/v1/inventory-items',bearer=token(expired=True)),'reject expired token')
    expect(403,request('POST','/api/v1/inventory-items',{'sku':'A','name':'A','category':'A','minimumStock':0,'unitPrice':0},token(role='SALES_CLERK')),'reject read-only write')
    item=create();sku=item['sku'];path=f"/api/v1/inventory-items/{item['id']}"
    expect(409,request('POST','/api/v1/inventory-items',{'sku':sku.lower(),'name':'Other','category':'Food','minimumStock':0,'unitPrice':0}),'database-backed duplicate SKU rejection')
    other_org=token(str(uuid.uuid4()))
    expect(404,request('GET',path,bearer=other_org),'isolate item reads by organization')
    create(sku,other_org)
    item=receive(item);lot=item['lots'][0]['id']
    persisted=expect(200,request('GET',path),'reload persisted product')
    assert persisted['totalQuantity']==20
    command=mutation(item,lotId=lot,type='Issue',quantity=4,reason='SALE-TEST')
    item=expect(200,request('POST',path+'/movements',command),'issue stock')
    replay=expect(200,request('POST',path+'/movements',command),'replay same operation safely')
    assert replay['totalQuantity']==16 and replay['version']==item['version'] and len(replay['movements'])==2
    expect(409,request('POST',path+'/movements',{**command,'quantity':5}),'reject reuse with changed payload')
    expect(409,request('POST',path+'/movements',mutation(item,lotId=lot,type='Issue',quantity=99,reason='Too much')),'reject insufficient stock')
    expect(400,request('POST',path+'/movements',mutation(item,lotId=lot,type='Issue',quantity=-1,reason='Invalid')),'reject negative issue')
    item=expect(200,request('PATCH',path+f'/lots/{lot}/quarantine',mutation(item,isQuarantined=True,reason='Inspection')),'quarantine lot')
    assert item['availableQuantity']==0 and item['totalQuantity']==16
    error=expect(409,request('POST',path+'/movements',mutation(item,lotId=lot,type='Issue',quantity=1,reason='Sale'),language='es-419'),'block quarantined issue')
    assert 'cuarentena' in error['detail'],error
    item=expect(200,request('PATCH',path+f'/lots/{lot}/quarantine',mutation(item,isQuarantined=False,reason='Inspected')),'release quarantine')
    item=expect(200,request('POST',path+'/counts',mutation(item,lotId=lot,expectedQuantity=16,countedQuantity=14,reason='Physical count')),'apply physical count atomically')
    assert item['totalQuantity']==14 and item['counts'][0]['countedQuantity']==14
    item=expect(200,request('POST',path+'/returns',mutation(item,sourceLotId=lot,quantity=2,disposition='Quarantine',reference='SALE-TEST',reason='Inspect returned goods')),'classify returned merchandise')
    assert item['totalQuantity']==16 and item['availableQuantity']==14 and len(item['lots'])==2
    item=expect(200,request('PATCH',path+f'/lots/{lot}/location',mutation(item,location='B-2',reason='Reorganization')),'persist location change')
    listing=expect(200,request('GET','/api/v1/inventory-items?search='+sku),'search exact SKU')
    assert listing['total']==1 and listing['items'][0]['id']==item['id']
    expect(400,request('GET','/api/v1/inventory-items?pageSize=1000'),'bound query pagination')
    expect(404,request('POST',path+'/movements',mutation(item,lotId=str(uuid.uuid4()),type='Waste',quantity=1,reason='Unknown lot')),'reject foreign lot identifier')
    concurrent_item=receive(create()); concurrent_path=f"/api/v1/inventory-items/{concurrent_item['id']}/movements"
    commands=[mutation(concurrent_item,lotId=concurrent_item['lots'][0]['id'],type='Issue',quantity=14,reason=f'SALE-{i}') for i in range(2)]
    with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:
        results=list(pool.map(lambda body:request('POST',concurrent_path,body),commands))
    assert sorted(r[0] for r in results)==[200,409],results
    after=expect(200,request('GET',concurrent_path.removesuffix('/movements')),'reload after concurrent sale attempts')
    assert after['totalQuantity']==6 and len(after['movements'])==2
    schema=expect(200,request('GET','/openapi/v1.json'),'serve OpenAPI document')
    assert '/api/v1/inventory-items/{id}/counts' in schema['paths']
    assert schema['components']['securitySchemes']['Bearer']['scheme']=='bearer'
    assert schema['paths']['/api/v1/inventory-items']['post']['responses']['201']['content']['application/json']['schema']
    print('PASS concurrent writes keep one deduction and roll back the losing transaction')
    print(json.dumps({'status':'passed','organizationId':ORG,'sampleItemId':item['id'],'concurrentItemId':after['id']}))
if __name__=='__main__':main()
