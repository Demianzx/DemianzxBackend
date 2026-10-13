# Recuperación de contraseña y correo SMTP

El login incluye **Olvidé mi contraseña**. El usuario ingresa su correo registrado, recibe un enlace a `/reset-password` y define una contraseña nueva sin recordar la anterior. La recuperación usa ASP.NET Identity; no requiere migraciones de base de datos.

## Configuración del backend

Configura estas variables en el proceso que ejecuta el backend o en los secretos de tu plataforma de despliegue. Los valores siguientes son ejemplos; sustituye el dominio, el servidor y las credenciales por los de tu proveedor SMTP.

```text
PasswordRecovery__ResetUrl=https://tu-blog.example/reset-password
Email__Smtp__Host=smtp.tu-proveedor.example
Email__Smtp__Port=587
Email__Smtp__EnableSsl=true
Email__Smtp__UserName=usuario-smtp
Email__Smtp__Password=secreto-smtp
Email__Smtp__FromAddress=no-reply@tu-blog.example
Email__Smtp__FromName=DemianzxGames
DataProtection__KeysPath=/ruta/al/volumen-persistente/identity-keys
```

- `ResetUrl` debe apuntar al frontend, ser una URL HTTPS absoluta y no tener parámetros, fragmento ni credenciales. No se toma una URL enviada por el navegador. En Development se permite HTTP en localhost; ya está configurado `http://localhost:3000/reset-password`.
- SMTP usa STARTTLS (habitualmente puerto 587). El cliente no soporta TLS implícito del puerto 465. El proveedor debe habilitar autenticación SMTP; usa una credencial o contraseña de aplicación si lo exige. También se admiten relays sin autenticación dejando usuario y contraseña vacíos.
- El remitente debe estar autorizado por tu proveedor. Configura SPF y DKIM en el dominio para mejorar la entrega.
- Mantén contraseñas SMTP fuera del repositorio. Los archivos `appsettings` incluyen únicamente campos vacíos y valores predeterminados para este servicio.
- En contenedores, guarda las claves de Data Protection en un volumen persistente; todas las réplicas deben compartirlo. Protege el directorio mediante permisos y cifrado del almacenamiento. Si se pierden las claves, los enlaces pendientes dejan de funcionar. Cuando `KeysPath` está vacío, se utiliza el almacenamiento predeterminado de ASP.NET Core.

Sin un host SMTP, remitente o `ResetUrl` válidos, la solicitud devuelve `503` tanto para correos existentes como para desconocidos. El resto de la aplicación continúa funcionando.

## Frontend

Configura `NEXT_PUBLIC_API_URL` con la URL pública del backend **al compilar Next.js** (por defecto `http://localhost:8080`). Agrega el origen del frontend a `Cors:AllowedOrigins` en el backend.

Las rutas nuevas son `/forgot-password` y `/reset-password`. También puedes solicitar el enlace desde el modal de inicio de sesión. El correo viene del enlace recibido y la API valida el token asociado a ese usuario antes de cambiar la contraseña.

## API

```http
POST /api/Users/forgot-password
Content-Type: application/json

{"email":"usuario@example.com"}
```

Responde `200` sin devolver el token. El mensaje del frontend siempre indica: “Si existe una cuenta con ese correo, recibirás un enlace”. Todas las solicitudes válidas entran en una cola antes de consultar la cuenta, para que el tiempo de entrega SMTP no revele qué correos están registrados.

```http
POST /api/Users/reset-password
Content-Type: application/json

{"email":"usuario@example.com","token":"token-del-enlace","newPassword":"NuevaClave123!"}
```

Responde `200` si se cambió la contraseña o `400` si el enlace no es válido, expiró o la contraseña incumple la política. No exige una sesión ni la contraseña anterior. Los tokens se codifican con Base64 URL-safe y vencen en una hora. Un cambio exitoso invalida todos los tokens de recuperación anteriores de ese usuario.

La nueva contraseña requiere al menos ocho caracteres en el comando y cumple además la política de Identity vigente (por defecto mayúscula, minúscula, número y carácter especial).

Las dos rutas comparten un límite de cinco solicitudes por IP cada quince minutos; el exceso devuelve `429`. El límite es local a cada instancia. Detrás de un proxy, configura correctamente las IP de proxies confiables y el procesamiento de forwarded headers en el despliegue para identificar la IP del cliente; no confíes en cabeceras enviadas libremente por el usuario. Con múltiples réplicas, aplica también un límite compartido en el gateway.

## Entrega y operación

El envío se procesa en segundo plano, en una cola en memoria limitada a 100 solicitudes. Si la cola está llena, se responde `503`. Un reinicio puede perder solicitudes pendientes; el usuario puede solicitar otro enlace. Para entrega durable en múltiples instancias, sustituye esta cola por un servicio persistente de mensajes.

Un fallo de conexión, credenciales o entrega SMTP se registra sin correo, token ni contenido del mensaje. La respuesta pública sigue siendo genérica; revisa los logs del backend y el panel de entregas del proveedor si los correos no llegan. La aceptación SMTP no garantiza que el mensaje llegue a la bandeja de entrada.

Los JWT nuevos incluyen el security stamp de Identity. Cada petición autenticada verifica que coincide con el usuario en la base de datos. Restablecer o cambiar la contraseña invalida las sesiones anteriores. **Al desplegar esta actualización, los JWT antiguos sin ese dato dejan de ser válidos y los usuarios deberán iniciar sesión de nuevo.** Esta comprobación agrega una consulta de usuario a las peticiones autenticadas.

Los logs de aplicación no incluyen el cuerpo de los comandos para evitar guardar contraseñas o tokens. La página de restablecimiento declara `no-referrer`, no se indexa y elimina los parámetros de la URL después de un cambio exitoso. Configura el proxy y cualquier herramienta de analítica para no registrar los parámetros del enlace.

## Verificación

```powershell
dotnet build src/Web/Web.csproj -p:SkipNSwag=True
dotnet test tests/Application.UnitTests/Application.UnitTests.csproj
```

En el frontend:

```powershell
npx tsc --noEmit --incremental false
npm run build -- --no-lint
```

Las pruebas incluyen entrega HTML mediante un servidor SMTP local, enlaces utilizables, correos desconocidos, tokens vencidos o alterados, un solo uso, cambio del security stamp y contraseña débil.

Para probar con correos reales, configura tu proveedor, registra una cuenta cuyo buzón controles, solicita el enlace y comprueba que llega. Después del cambio, verifica que la contraseña anterior y el enlace usado ya no funcionan y que una sesión anterior recibe `401`. No se han enviado correos a personas reales desde las pruebas automatizadas.

Referencia: [recuperación de contraseña y tokens de ASP.NET Identity](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/account-confirmation-and-password-recovery?view=aspnetcore-9.0).
