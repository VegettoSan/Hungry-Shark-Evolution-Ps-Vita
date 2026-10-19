# Trabas de menús y preparación 3D — 2026-10-18

**Resultado:** dos fuentes privadas corregidas y un helper de metadatos añadido.
Unity 2018.2.19f1 / PSP2, `main`, excepción específica del encargo; P03 ACTIVE,
3/13 aprobadas. El propietario informa una pequeña mejora de gameplay con overclock
en la VPK anterior, sin nueva medición numérica; las trabas de menús persistían.

## Causa observada y correcciones

**VERIFIED:** `GUIShark.CreateAttachPoints` cargaba/clonaba tiburones completos,
ejecutaba su inicialización, copiaba nueve anclajes y destruía cada clon. Se repetía
al construir selección, tienda y pantallas anteriores. El getter `modelPrefab`
siempre usa una ruta Resources fija por `sharkType`; `Player.Awake` no cambia esos
anclajes. No hacía falta repetir esa preparación para contenido inmutable.

En PSP2 se conservan como máximo once entradas de **posiciones, rotaciones y nombres**,
con clave de tipo de tiburón/modelo. El primer uso obtiene los datos mediante la
inicialización original. Los siguientes crean los anclajes necesarios sin volver a
cargar/clonar el player. No se retienen prefabs, meshes, materiales, transforms ni
tiburones vivos. El coste inicial no desaparece ni se proclama mejor arranque frío.

**VERIFIED:** `LabCrane.Initialise` construía los nueve especiales al entrar, aunque
solo tres eran visibles. PSP2 prepara los tres solicitados por el carrusel original;
los demás se crean cuando se necesitan. Conserva como máximo cinco instancias
referenciadas, tres visibles y hasta dos aparcadas. Expulsa solo modelos inactivos
y libera el material que creó; todos pertenecen al menú y desaparecen al cerrarlo.
`Destroy` es diferido: puede haber objetos pendientes hasta finalizar el frame.
No se cambia la selección, elegibilidad, escala, animación ni lógica de compra.

Ambas correcciones tienen reducción objetiva de trabajo comprobada en Editor
(categoría 1) y fundamento de memoria/CPU para Vita (categoría 2). El efecto físico
en tiempos/FPS/RAM sigue pendiente (categoría 3).

## Comparación local

Tiempo síncrono de la acción/constructor, no duración total de animaciones ni Vita:

| Ruta equivalente en Editor | Antes | Después |
| --- | ---: | ---: |
| Principal → Evolución | 67,5 ms | 54,8 ms |
| Evolución → tienda | 803,8 ms | 526,5 ms; otra sesión 511,3 ms |
| Tienda → Evolución | 370,0 ms | 80,2 ms; otra sesión 77,1 ms |
| Evolución → especiales | 2333,6 ms | 1009,2 ms; otra sesión 1082,0 ms |
| Especiales → Evolución | 331,4 ms | 38,9 ms; otra sesión 39,3 ms |

El desglose previo atribuye 2286 ms a `LabCrane.Initialise`, 320 ms a preparación
de tiburón en tienda y 441 ms a construir sus filas. GC forzado: muestras 5–8 ms;
`UnloadUnusedAssets` medido aquí solo al **solicitarlo**, no su coste asíncrono total.
Se conservan ambas limpiezas originales. Los promedios de FPS no evalúan estas rutas.

Hipótesis descartada: 50 inicializaciones de icono usaban ruta vacía y sumaban
0,08 ms; no eran 50 cargas costosas. No se modifica su orden de activación.
La UI de tienda todavía consume aproximadamente 440 ms y requiere otro análisis.

## Validación y límites

- **PASS:** 198 anclajes de Evolución y 198 de tienda coinciden byte a byte con los
  recibos previos: nombre, padre, posición, rotación y escala, incluidos bebés.
- **PASS:** tres modelos al entrar, cinco tras navegación y cero después de cerrar.
  Dos tandas de veinte pasos del carrusel, ambos sentidos, límites y escalas
  originales de los tres modelos visibles. Primera visita a modelos no preparados
  todavía puede costar una carga: veinte pasos forzados juntos tomaron ~0,9 s frente
  a ~5 ms cuando todos estaban precargados. **No son latencia de una pulsación**;
  esta transferencia de coste exige probar primeras selecciones en Vita.
- **PASS:** diálogo de cosmético y cierre, seis iconos visibles cargados y otros
  seis tras desplazar la tienda; varias reconstrucciones, regreso y gameplay con
  movimiento, alimentación, pausa y reanudación. No nuevas excepciones de producción
  en estas rutas; persisten IAP no disponible y avisos conocidos de limpieza.
- **Compra confirmada NO ejecutada:** la revisión automática rechazó cambiar
  moneda/inventario sin autorización específica. Se validó sin compra y se conserva
  intacto el código de precios, débito, adquisición, equipamiento y guardado.
- **PASS:** C#4/Mono PSP2 0 errores / 271 advertencias y Editor 0/269, antes y después.
  Dieciocho comparaciones de tokens conservan otras plataformas. Entre 31.725 métodos,
  cambian cinco cuerpos de `GUIShark`/`LabCrane` y se añaden cinco métodos del helper
  y su preparación/limpieza. Ningún método de gameplay o compra cambia.
- Una primera ronda posterior aún usó el ensamblado viejo del Editor; se conserva
  como intento inválido. Se forzó Refresh y comprobó la presencia del helper antes
  de las mediciones posteriores incluidas en la tabla. El primer diagnóstico de
  cosmético también falló por usar el manager de gameplay fuera de partida; corregido.

**PASS build nativo:** 18 escenas, Mono2x, 2:01,2, cero errores / 296 advertencias;
sin nuevas advertencias C#. Auditorías de menú, recursos, optimizaciones anteriores,
calidad, guardado y ausencia de overlays pasan. El control anterior es rechazado
correctamente por carecer del nuevo camino de metadatos.

**PASS integridad:** entre 19.648 archivos originales solo difieren `GUIShark.cs` y
`LabCrane.cs`; se añade el helper con su meta. Ajustes, escenas y demás fuentes
coinciden. Ocho archivos de perfil restaurados byte a byte; diagnósticos archivados.
El offset animado incidental de `Underwater.mat` se preservó y restauró desde el
backup exacto; no hay cambio de material/renderizado en las fuentes finales.

**VPK privada nueva:** `HSE_Menu_Optimized_HGSKE1178.vpk`, 135.086.465 bytes,
213 entradas, SHA-256 `6a81d4404aebaea29055c58305f598673f033fbc1710be691e9d4d98ac33a5ad`.
CRC, igualdad byte a byte con el player, identidad HGSKE1178 y artwork pasan;
sin savedata ni diagnósticos temporales. VPK anterior intacta; sin instalación,
publicación o prueba física. Finalización: 2026-10-19 UTC, 18 en Bogotá.
[Recibo saneado](evidence/MENU_MODEL_2026-10-18.json).

Evidencia privada: `Reports/MenuStutter_20261018/`, backups, diffs, recibos,
`TimingSummary.json`, `PoseContracts.json`, `BranchParity.json` y `MethodAudit.json`.
[Auditor del player](../tools/audit_hse_menu_models.ps1).

**Próxima prueba física:** comparar la VPK anterior y nueva con el mismo perfil y
frecuencias; medir entrada/regreso y primera selección/repetición de cada especial,
cosméticos, memoria máxima y varias vueltas sin acumulación. La menor preparación
inicial no demuestra todavía ausencia de trabas, ahorro físico de RAM ni más FPS.
