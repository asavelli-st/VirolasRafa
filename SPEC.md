# SPEC – Sistema de presupuestos, órdenes de armado, cuenta corriente y cobranzas

> Pegar este archivo en la raíz de la solución (como `SPEC.md`) y referenciarlo desde `CLAUDE.md`.
> Referencia visual del armado de presupuesto: `docs/prototipo-presupuesto.html` (abrir en el navegador, funciona sin servidor).

## 1. Objetivo

Sitio web responsive (se usa mucho desde el celular) para:

1. Armar **presupuestos** a partir de una lista de precios precargada.
2. Al **aprobarlos**, generar una **orden de armado** y registrar el débito en la **cuenta corriente** del cliente.
3. Registrar **cobranzas** que reducen el saldo del cliente.
4. Generar **PDF** del presupuesto (para el cliente) y de la orden de armado (para quien arma).

Es la **V1**: lo mínimo para empezar a usarlo. Priorizar simplicidad y corrección sobre cantidad de funciones.

## 2. Stack y convenciones

- **ASP.NET Core MVC** (controladores + vistas Razor). **No** usar Razor Pages.
- **.NET LTS vigente** (usar la que tenga instalada el SDK; verificar con `dotnet --version`).
- **Entity Framework Core + SQL Server** (LocalDB para desarrollo). Migraciones con `dotnet ef`.
- **ASP.NET Core Identity** para login, con roles **Admin** y **Vendedor**.
- **Bootstrap 5.3** (con modo claro/oscuro nativo vía `data-bs-theme`) y **Font Awesome** para iconos. Servir las librerías localmente (LibMan o npm), no desde CDN.
- **QuestPDF** para los PDF. Antes de usarlo, avisarme para que verifique las condiciones de licencia Community. Alternativa si no sirve: PDFsharp/MigraDoc.
- **ClosedXML** (MIT) para leer el Excel de importación.
- **xUnit** para tests de la lógica de negocio.
- Una sola solución, un solo proyecto web, con esta estructura:

```
/Controllers
/Models          (entidades)
/ViewModels
/Data            (DbContext, Migrations, Seed)
/Services        (lógica de negocio)
/Pdf             (documentos QuestPDF)
/Views
/tests           (proyecto xUnit)
/docs            (prototipo HTML)
```

- Idioma de la interfaz: **español (es-AR)**. Fechas `dd/MM/yyyy`, moneda en pesos con formato `es-AR`. Configurar `RequestLocalization`.
- Nombres de clases y propiedades en español, consistentes con este documento.
- Importes siempre `decimal(18,2)`. Porcentajes `decimal(5,2)`. Nunca `float`/`double`.
- **Toda la lógica de negocio va en `/Services`**, no en controladores ni en vistas.
- **Nunca confiar en los totales que manda el navegador**: el servidor recalcula precios, descuentos y totales al guardar.

## 3. Roles y permisos

| Función | Admin | Vendedor |
|---|---|---|
| Clientes (alta/edición) | Sí | Sí |
| Productos (ABM, importación) | Sí | Solo lectura, **sin costo ni margen** |
| Presupuestos (crear, editar borrador, enviar, aprobar, rechazar) | Sí | Sí |
| Anular presupuesto aprobado / cancelar orden | Sí | No |
| Órdenes de armado (cambiar estado) | Sí | Sí |
| Cobranzas (registrar) | Sí | Sí |
| Anular cobranza | Sí | No |
| Cuenta corriente (consulta) | Sí | Sí |
| Usuarios | Sí | No |

- **Costo y margen no deben viajar al navegador del Vendedor.** Usar ViewModels distintos o proyecciones en la consulta; no alcanza con ocultar columnas en la vista.
- No hay registro público. El Admin crea usuarios. Un usuario inicial Admin se crea en el seed, con email y contraseña tomados de configuración (User Secrets), nunca escritos en el código.
- Todas las pantallas requieren login salvo la de login. Usar `[Authorize]` por defecto (política global) y `[Authorize(Roles = "Admin")]` donde corresponda.
- Los usuarios se desactivan, no se borran.

## 4. Modelo de datos

### Cliente
`Id`, `Nombre` (obligatorio), `Documento` (CUIT/DNI, opcional), `Telefono`, `Email`, `Direccion`, `DescuentoHabitualPct` (0–100, default 0), `Observaciones`, `Activo`.

### Producto
`Id`, `Codigo` (string, **único**, es el ID de la lista de precios), `Descripcion`, `Costo`, `Precio`, `Cantidad2?`, `Precio2?`, `Cantidad3?`, `Precio3?`, `Activo`.

- **Margen no se guarda**: es una propiedad calculada `(Precio - Costo) / Costo * 100`. Si `Costo = 0`, el margen es nulo y se muestra vacío.
- Se calcula igual para cada escalón (Precio2, Precio3), solo para el Admin.
- Validaciones: `Cantidad2 > 1`; `Cantidad3 > Cantidad2`; si hay cantidad, hay precio, y viceversa; el escalón 3 no puede existir sin el 2.
- En el formulario, el Admin carga **margen o precio** y el otro se recalcula en vivo según el último campo editado (JS). Al guardar, el servidor persiste `Costo` y `Precio`; si llegó margen, calcula `Precio = Costo × (1 + Margen/100)` redondeado a 2 decimales.
- No se borran productos: se desactivan (los presupuestos viejos los referencian).

### Presupuesto
`Id`, `Numero` (correlativo, único), `Fecha`, `ClienteId`, `Estado` (Borrador, Enviado, Aprobado, Rechazado, Anulado), `DescuentoTipo` (Porcentaje | Monto), `DescuentoValor`, `Subtotal`, `DescuentoImporte`, `Total`, `ValidezDias` (default 7), `Observaciones`, `CreadoPorId`, `FechaCreacion`, `FechaAprobacion?`.

### PresupuestoItem
`Id`, `PresupuestoId`, `ProductoId`, y **copia (snapshot)** de: `Codigo`, `Descripcion`, `CostoUnitario`, `PrecioUnitario`; más `Cantidad` e `Importe` (`Cantidad × PrecioUnitario`).

- Guardar `CostoUnitario` permite calcular márgenes reales históricos aunque cambie la lista.

### OrdenArmado
`Id`, `Numero` (correlativo), `PresupuestoId` (1 a 1), `Fecha`, `Estado` (Pendiente, EnArmado, Armada, Entregada, Cancelada), `Observaciones`.

### OrdenArmadoItem
`Id`, `OrdenArmadoId`, `Codigo`, `Descripcion`, `Cantidad`. **Sin precios.**

### MovimientoCuentaCorriente
`Id`, `ClienteId`, `Fecha`, `Tipo` (Debito | Credito), `Importe` (siempre positivo), `Concepto`, `Origen` (Presupuesto | Cobranza | Anulacion), `OrigenId`, `MovimientoAnuladoId?`, `CreadoPorId`.

- **El saldo no se guarda**: `Saldo = Σ Débitos − Σ Créditos`. Saldo positivo = el cliente debe.
- Los movimientos **nunca se editan ni se borran**. Cualquier corrección es un contramovimiento.

### Cobranza
`Id`, `Numero` (correlativo), `Fecha`, `ClienteId`, `Importe` (> 0), `FormaPago` (Efectivo, Transferencia, Cheque, Tarjeta, Otro), `Observaciones`, `Anulada`, `MovimientoId`, `CreadoPorId`.

- No se imputa a comprobantes: solo reduce el saldo general del cliente.

### Numeración
Presupuestos, órdenes y cobranzas tienen número correlativo propio. Generarlo de forma segura ante concurrencia (secuencia de SQL Server o tabla de contadores dentro de la transacción).

## 5. Reglas de negocio

### 5.1 Precio por cantidad (automático)
Para un ítem con cantidad `q`:
- `q >= Cantidad3` (si existe) → `Precio3`
- si no, `q >= Cantidad2` (si existe) → `Precio2`
- si no → `Precio`

Aplica la **cantidad del ítem**, no la suma de productos distintos. El escalón aplica **desde** esa cantidad inclusive. El vendedor **no puede editar el precio** de un ítem en la V1. Implementar en un único método puro y testeado (`PrecioPorCantidad`).

### 5.2 Descuento total del presupuesto
- Tipo Porcentaje (0–100) o Monto (≥ 0, no mayor al subtotal).
- Al elegir cliente, si tiene `DescuentoHabitualPct > 0`, se **precarga** como descuento total en porcentaje. El vendedor puede cambiarlo o quitarlo.
- `Subtotal = Σ Importe`; `DescuentoImporte` según tipo; `Total = Subtotal − DescuentoImporte`. Redondear a 2 decimales en un solo lugar.
- No hay IVA discriminado: los precios son finales.

### 5.3 Estados del presupuesto
```
Borrador → Enviado → Aprobado
                  ↘ Rechazado
Borrador → Aprobado (permitido directo)
Aprobado → Anulado (solo Admin)
```
- Solo el **Borrador** es editable. Un presupuesto Enviado puede volver a Borrador para corregirlo.
- Debe tener al menos un ítem y un cliente para pasar a Enviado o Aprobado.

### 5.4 Aprobación (transacción única)
Al aprobar, en **una sola transacción**:
1. Cambiar estado a Aprobado y fijar `FechaAprobacion`.
2. Crear `OrdenArmado` (estado Pendiente) con los ítems copiados, sin precios.
3. Crear `MovimientoCuentaCorriente` tipo **Débito** por el `Total`, concepto "Presupuesto N° X".

Si algo falla, no se guarda nada. Aprobar dos veces el mismo presupuesto no debe duplicar nada (idempotencia: verificar el estado dentro de la transacción).

### 5.5 Anulación de presupuesto aprobado (Admin)
Estado Anulado, orden Cancelada y **contramovimiento** (Crédito, origen Anulacion, apuntando al movimiento original). No se permite si la orden ya está Entregada, salvo confirmación explícita del Admin.

### 5.6 Orden de armado
`Pendiente → EnArmado → Armada → Entregada`. Cancelar (solo Admin) desde cualquier estado menos Entregada, con el mismo contramovimiento que 5.5.

### 5.7 Cobranzas
- Registrar crea la `Cobranza` y un `Movimiento` **Crédito** en la misma transacción.
- Anular (solo Admin): marca `Anulada = true` y crea un contramovimiento (Débito, origen Anulacion). Nunca se borra.
- Se permite que el saldo quede a favor del cliente (negativo); mostrarlo como "Saldo a favor".

### 5.8 Importación de productos desde Excel
- Pantalla con subida de `.xlsx` → **vista previa** (nuevos / a actualizar / con error) → botón Confirmar.
- Columnas: `Codigo`, `Descripcion`, `Costo`, `Margen` **o** `Precio`, y opcionales `Cantidad2`, `Precio2`, `Cantidad3`, `Precio3`.
- Si vienen `Precio` y `Margen`, **gana `Precio`**. Si viene solo `Margen`, `Precio = Costo × (1 + Margen/100)`.
- Crea los códigos nuevos y **actualiza** los existentes por `Codigo`. **Nunca borra** ni desactiva productos que no estén en el archivo.
- Reportar errores por fila (código vacío, número inválido, escalones inconsistentes). Las filas con error no se importan; las válidas sí, si el Admin confirma.
- Ofrecer una **plantilla Excel descargable** con las columnas.
- Solo Admin.

## 6. Pantallas (V1)

1. **Login** / cerrar sesión.
2. **Dashboard**: saldo total a cobrar, presupuestos Enviados pendientes de respuesta, órdenes Pendientes y En armado, últimas cobranzas.
3. **Clientes**: listado con búsqueda y saldo; alta/edición; **detalle con cuenta corriente** (movimientos ordenados por fecha, saldo acumulado y saldo actual) y botón "Registrar cobranza".
4. **Productos**: listado con búsqueda; ABM; importación desde Excel. (Vendedor: solo consulta, sin costo ni margen.)
5. **Presupuestos**:
   - Listado con filtro por estado, cliente y fechas.
   - **Alta/edición**: seguir el prototipo `docs/prototipo-presupuesto.html`. Selector de cliente con descuento por defecto, buscador de productos (por código o descripción) con resultados que se tocan para agregar, cantidad con botones − / + y campo numérico, aviso del próximo escalón de precio, descuento total en % o $, resumen con subtotal, descuento y total. En celular: barra inferior fija con total y botón de guardar.
   - **Detalle** con cambios de estado y botón **Descargar PDF**.
6. **Órdenes de armado**: listado y detalle con cambio de estado y botón **Descargar PDF**.
7. **Cobranzas**: listado y alta (cliente, fecha, importe, forma de pago, observaciones). Admin puede anular.
8. **Usuarios** (Admin): alta, rol, activar/desactivar, resetear contraseña.

### UI transversal
- **Mobile-first.** Todo debe ser usable a 360 px de ancho; las tablas anchas hacen scroll horizontal dentro de su contenedor, o se muestran como tarjetas en pantallas chicas.
- Elementos táctiles de al menos 44 px de alto.
- **Selector de tema Auto / Claro / Oscuro** en la barra superior. Auto sigue el sistema; se implementa con `data-bs-theme` en `<html>` y se recuerda en `localStorage`. Evitar un parpadeo inicial aplicando el tema con un script corto en el `<head>`.
- Confirmación antes de acciones irreversibles (aprobar, anular, cancelar).
- Mensajes de error claros y en español.

## 7. PDF

Generados en el servidor con QuestPDF, tamaño A4, con descarga desde el detalle. Datos de la empresa (nombre, domicilio, teléfono, email, logo opcional) tomados de la sección `Empresa` de `appsettings.json`, con placeholders `[NOMBRE DE LA EMPRESA]` hasta que los complete.

**Presupuesto (para el cliente)**
Encabezado de empresa, número, fecha y validez, datos del cliente, tabla de ítems (código, descripción, cantidad, precio unitario, importe), subtotal, descuento (si hay), **total**, observaciones. Sin costos ni márgenes.

**Orden de armado (para quien arma)**
Encabezado, número de orden, número de presupuesto, fecha, cliente (solo nombre), tabla de ítems con **código, descripción, cantidad y un casillero para tildar**. **Sin precios ni totales.** Espacio para observaciones.

Nombre de archivo sugerido: `Presupuesto-0001.pdf`, `OrdenArmado-0001.pdf`. Verificar que el botón funcione en celular (descarga o abre en el visor).

## 8. Fuera de alcance en la V1

IVA discriminado, envío por mail desde el sistema, stock, múltiples listas de precios, imputación de cobranzas a comprobantes, edición manual del precio de un ítem, duplicar presupuestos, reportes avanzados. Mantener el modelo abierto para sumarlos después.

## 9. Plan por etapas

Trabajar **una etapa por vez**, con `dotnet build` sin errores ni warnings nuevos, y **un commit por etapa**. Al terminar cada una, resumir qué se hizo y qué se probó, y esperar mi OK antes de seguir.

1. **Esqueleto y login**: solución, EF Core con SQL Server, Identity, roles, seed del Admin, layout con Bootstrap, Font Awesome y selector de tema, política global de autorización, pantalla de usuarios.
2. **Clientes y Productos**: ABM de ambos, cálculo margen/precio, precios por escalón, importación desde Excel con vista previa y plantilla. Ocultar costo y margen al Vendedor desde el servidor.
3. **Presupuestos**: servicio de cálculo (precio por cantidad, descuento, totales) con tests, pantalla de alta/edición según el prototipo, listado y detalle, flujo de estados hasta Enviado/Rechazado.
4. **Aprobación, orden de armado y cuenta corriente**: transacción de aprobación, órdenes con sus estados, movimientos, anulaciones con contramovimiento, detalle de cuenta corriente por cliente.
5. **Cobranzas y Dashboard**: alta y anulación de cobranzas, saldos, dashboard.
6. **PDF**: presupuesto y orden de armado, botones de descarga.
7. **Pulido**: revisión en pantalla de 360 px, validaciones, mensajes, permisos de punta a punta y datos de ejemplo para demo.

## 10. Criterios de aceptación

- Un Vendedor no puede ver costo ni margen en ninguna pantalla ni respuesta, ni manipulando URLs.
- Cambiar el precio de un producto **no altera** presupuestos ya guardados.
- El precio por cantidad y los totales se recalculan en el servidor y coinciden con lo que muestra la pantalla.
- Aprobar un presupuesto crea exactamente **una** orden y **un** débito; reintentar no duplica.
- El saldo de un cliente es siempre la suma de sus movimientos y se puede reconstruir desde ellos.
- Anular una cobranza o un presupuesto aprobado deja el saldo correcto y conserva el historial.
- La importación nunca borra productos y reporta los errores por fila.
- Los PDF se generan sin precios en la orden de armado y sin costos en el presupuesto.
- Todo el flujo funciona en un celular y en escritorio, en modo claro y oscuro.
- Los tests de la lógica de negocio pasan: precio por cantidad (bordes de cada escalón), descuentos (% y monto, tope), totales, saldo, aprobación idempotente, anulaciones.

## 11. Cómo trabajar conmigo

- Antes de cada etapa, si hay una decisión de diseño que no esté en este documento, preguntame en vez de asumirla.
- No agregues paquetes NuGet que no estén listados sin avisarme.
- No guardes secretos en el repositorio.
- Si algo de este documento se contradice o es inviable, decímelo y proponé una alternativa.
