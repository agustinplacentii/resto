# Restaurant Local Orders

Aplicacion local para tomar pedidos de restaurante, controlar stock y cambiar precios.

## Requisitos

- .NET 10 o superior
- Node.js 20 o superior
- Docker Desktop para PostgreSQL

## Base de datos

La app usa tu PostgreSQL local con la base `restaurant_local`.
La base arranca vacia: primero se crean categorias propias y despues productos asociados.

## Backend

```powershell
cd api
dotnet restore
dotnet run
```

API: http://localhost:5088

## Endpoints principales

- `GET /api/products/groups`: categorias/cards principales, por ejemplo Pizzas, Cervezas y Tragos.
- `POST /api/products/groups`: crear una categoria.
- `GET /api/products`: todos los productos.
- `GET /api/products?groupId=1`: productos relacionados a una categoria.
- `GET /api/products/{id}`: detalle de un producto.
- `POST /api/products`: crear producto.
- `PUT /api/products/{id}`: actualizar precio, stock, medida o estado.
- `GET /api/orders`: pedidos recientes.
- `GET /api/orders/{id}`: detalle de un pedido.
- `POST /api/orders`: guardar pedido y descontar stock.
- `PATCH /api/orders/{id}/status`: cambiar estado del pedido.
- `GET /api/orders/{id}/invoice.pdf`: factura imprimible en PDF.

## Frontend

```powershell
cd client
npm install
npm run dev
```

App: http://localhost:5173

## Datos de PostgreSQL

- Host: localhost
- Puerto: 5432
- Base: restaurant_local
- Usuario: postgres
- Password: postgres
