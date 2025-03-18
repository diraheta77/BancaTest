# Sistema de Estado de Cuenta de Tarjetas de Crédito

## Descripción
Sistema web para gestión de estados de cuenta de tarjetas de crédito, desarrollado con ASP.NET Core 8. Permite visualizar estados de cuenta, registrar compras y pagos, y configurar tasas de interés.

## Características Principales
- Visualización de estado de cuenta
- Registro de compras y pagos
- Cálculo automático de:
  - Saldo disponible
  - Interés bonificable
  - Pago mínimo
  - Pago de contado con intereses
- Configuración de tasas
- Exportación a PDF
- Listado de transacciones

## URLs del Sistema
### Para Pruebas
- **Frontend**: <a href="https://bancaweb-bsaafugqbybjf8fm.eastus2-01.azurewebsites.net/" target="_blank">BancaWeb</a>
- **API**: <a href="https://bancaminimalapi-cyg8gqguemecbngz.eastus2-01.azurewebsites.net/" target="_blank">Swagger Documentation</a>

## Documentación API
### Colección Postman
<a href="https://tinyurl.com/bwrydssb" target="_blank">
  <img src="https://run.pstmn.io/button.svg" alt="Collection for Postman">
</a>

La colección incluye todos los endpoints necesarios para:
- Gestión de estados de cuenta
- Registro de transacciones
- Configuración del sistema
- Ejemplos de peticiones

### Endpoints Principales
- `GET /api/creditcards/{id}` - Obtiene estado de cuenta
- `GET /api/creditcards/{id}/transactions` - Lista transacciones
- `POST /api/transactions/purchase` - Registra compra
- `POST /api/transactions/payment` - Registra pago
- `GET /api/configuration` - Obtiene configuración
- `PUT /api/configuration` - Actualiza configuración

## Tecnologías Utilizadas
- ASP.NET Core 8 (Minimal API + MVC)
- SQL Server
- Entity Framework Core
- AutoMapper
- FluentValidation
- iText7 (PDF)
- Bootstrap 5
- jQuery
- DataTables
- FontAwesome
- Toastr

## Seguridad y Optimización
### Rate Limiting
La API implementa límites de tasa para proteger los recursos y optimizar costos:
- Límite de 100 solicitudes por minuto por IP
- Respuesta 429 (Too Many Requests) al exceder el límite
- Headers de control X-Rate-Limit en respuestas
- Configuración personalizable via appsettings.json

Esta característica ayuda a:
- Prevenir abusos de la API
- Optimizar costos en Azure
- Mantener rendimiento óptimo
- Proteger contra ataques DoS

## Estructura del Proyecto
```
BancaTest/
├── BancaMinimalAPI/         # Backend API
│   ├── Features/            # Características organizadas por dominio
│   ├── Models/             # Modelos de dominio
│   └── Data/               # Contexto y configuración de BD
└── BancaWeb/               # Frontend MVC
    ├── Controllers/        # Controladores MVC
    ├── Models/            # ViewModels
    ├── Views/             # Vistas Razor
    └── Services/          # Servicios del cliente
```

## Configuración Local
1. Prerrequisitos:
   - .NET 8 SDK
   - SQL Server
   - Visual Studio 2022/VS Code

2. Base de datos:
```bash
cd BancaMinimalAPI
dotnet ef database update
```

3. Ejecución:
```bash
# Terminal 1 - API
cd BancaMinimalAPI
dotnet run

# Terminal 2 - Web
cd BancaWeb
dotnet run
```

## Flujo de Trabajo
1. Seleccionar tarjeta de crédito
2. Ver estado de cuenta
3. Realizar operaciones:
   - Registrar compras
   - Realizar pagos
   - Ver transacciones
   - Exportar estado de cuenta

## Configuración del Sistema
- Tasa de interés configurable (default: 25%)
- Tasa de pago mínimo configurable (default: 5%)

## Autores
- Diego Iraheta

## Licencia
Este proyecto es privado.