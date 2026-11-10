# IAM: registro e inicio de sesión — US06 y US07

## Recorrido disponible

1. Desde la landing, **Crear cuenta** abre el formulario de organización, nombre, correo y contraseña.
2. El registro público crea un administrador y una organización nueva. No permite elegir el rol ni acceder a una organización existente.
3. El usuario inicia sesión con correo y contraseña. Recibe un JWT de 60 minutos con identidad pública, organización y rol.
4. El administrador abre **Usuarios** para registrar operarios o vendedores dentro de su organización. La API obtiene esa organización de la identidad autenticada.
5. El administrador puede habilitar/deshabilitar cuentas. No puede deshabilitar su propia cuenta.
6. Las cuentas deshabilitadas no pueden ingresar; sus tokens anteriores también se rechazan en la siguiente petición protegida.
7. Al cerrar sesión se borra el token del navegador y se limpia el inventario de esa cuenta. El cierre es local al navegador: no existe revocación global por sesión; el token emitido conserva su validez hasta caducar o deshabilitar la cuenta.

## Contratos

Registro público, `POST /api/v1/authentication/sign-up`:

```json
{"organizationName":"Mi negocio","fullName":"Nombre del administrador","email":"owner@example.com","password":"YOUR_PASSWORD_AT_LEAST_12_CHARACTERS"}
```

Inicio de sesión, `POST /api/v1/authentication/sign-in`:

```json
{"email":"owner@example.com","password":"YOUR_PASSWORD_AT_LEAST_12_CHARACTERS"}
```

La respuesta incluye `user`, `token` y `expiresAt`. Los recursos de usuario no contienen hashes ni contraseñas. `GET /api/v1/authentication/me` valida y devuelve la cuenta actual.

Registro de personal, `POST /api/v1/users`, autorizado para `BUSINESS_OWNER`:

```json
{"fullName":"Nombre del operario","email":"operator@example.com","password":"YOUR_INITIAL_PASSWORD_AT_LEAST_12_CHARACTERS","role":"WAREHOUSE_OPERATOR"}
```

El otro rol admitido para personal es `SALES_CLERK`. `GET /api/v1/users` devuelve únicamente los usuarios de la organización del administrador. `PATCH /api/v1/users/{id}/status`, con `{"isActive":false}`, deshabilita una cuenta de esa misma organización. Los identificadores públicos de usuario son UUID; la clave interna de EF es `UserId : IEntityId<int>`.

## Reglas y persistencia

- Correo normalizado y único, protegido por índice de base de datos incluso ante registros simultáneos.
- Contraseña de al menos 12 caracteres y como máximo 72 bytes UTF-8. BCrypt guarda un hash con sal, nunca texto plano. Se conserva el contrato `IHashingService` y el adaptador BCrypt de la guía.
- Credenciales inválidas: 401. Cuenta deshabilitada con contraseña correcta: 403. Correo duplicado: 409. Campos inválidos: 400.
- JWT HS256 con validación de firma, emisor, audiencia y caducidad. La clave se obtiene de variables de entorno o secretos locales de .NET y debe tener al menos 32 caracteres. No está en el repositorio.
- Cada solicitud protegida comprueba también que el usuario exista, siga activo y conserve organización y rol. Se aplica un límite de 20 solicitudes IAM por minuto por dirección IP; un exceso devuelve 429.
- IAM tiene su propio `IamDbContext`, esquema `iam` e historial de migraciones. Usa las implementaciones originales de `BaseRepository`, `AppDbContext` y `UnitOfWork` de Shared. El host construye la unidad de trabajo de IAM con su contexto; no utiliza la unidad de trabajo de Warehouse para guardar usuarios.
- Domain contiene el agregado, reglas, comandos, consultas y contratos. Application coordina repositorios y puertos de hashing/token/identidad. Infrastructure implementa BCrypt, JWT, autenticación y EF. Interfaces contiene recursos REST y assemblers.

La validación de JWT comprueba las propiedades descritas en la [documentación oficial de ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication?view=aspnetcore-10.0). Su integración permanece dentro de IAM; los 26 archivos originales de Shared no se modificaron.

## Revisión realizada

Compilación C# y producción Vue correctas. Recorrido en navegador: crear organización, iniciar sesión, registrar un operario, cambiar a español, deshabilitarlo, comprobar el rechazo y cerrar sesión. Se comprobó también el rechazo de correo duplicado, contraseña incorrecta, acceso anónimo, escritura de inventario por vendedor, uso de un token después de deshabilitar la cuenta y acceso de otro administrador a productos/usuarios ajenos. Se usaron registros locales identificados como demostración; no se crearon proyectos ni archivos de tests.

Recuperación de contraseña, invitaciones por correo, cambio de contraseña inicial, edición de roles y gestión de suscripciones quedan fuera de este cambio. La publicación de código en GitHub no equivale a desplegar la aplicación.
