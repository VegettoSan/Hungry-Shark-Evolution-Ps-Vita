# Integración provisional del mando Vita en Unity 2018

Intervención solicitada expresamente por el propietario. **P00 ACTIVE; 0/13 fases aprobadas.** No acredita P09/P10, un player PSP2 ni controles físicos verificados. Código recuperado, perfiles del juego, copias de seguridad y pruebas detalladas permanecen privados.

## Arquitectura encontrada

**VERIFIED, inspección del proyecto recuperado:** `FGOLControllerMapper.CheckForControllers` combina nombres de joystick y detección nativa. Reconoce perfiles Amazon/NVIDIA y rutas Android/Remote; carga diccionarios de acciones lógicas a entradas del Input Manager. Conserva sus seis perfiles serializados y su enum. El mapper original comparte el objeto persistente `King` y debe reutilizarse.

`GameInput` convierte la entrada en `tiltX/tiltY`; el movimiento posterior pertenece a los sistemas originales. El turbo usa `GetTapPosition` y `GetTouchPositionForID`, consumidos por el control de impulso del jugador. No se modificaron velocidad, física, energía, recarga ni esas mecánicas.

`FGOLControlManager`, `FGOLScreenConfig`, las configuraciones direccionales y sus variantes dinámicas gestionan selección y acciones. `FGOLControlSharkScroller` cambia el índice del `GUIDragPanel` original. `UIButtonMessage`, `UIButtonKeys`, `OnKey` y el procesamiento press/release de `UICamera` proporcionan las rutas NGUI. `ControllerUpdateSelectedShark` corresponde al selector del laboratorio; su nombre no demuestra que controle el movimiento del jugador.

Muchas rutas de navegación se desactivaban fuera de `PlatformUtils.IsConsole`, que representa la plataforma Android TV. No se cambió globalmente esa propiedad: también interviene en servicios y otras decisiones ajenas a controles.

## Adaptación privada

**VERIFIED:** el perfil PSP2 adicional usa el Input Manager legado y conserva los perfiles existentes. Se activa con `UNITY_PSP2`; en Editor requiere activar explícitamente `Tools > HSE > Vita Controller Preview (next Play)`. Windows y Android conservan su ruta previa cuando el adaptador no está activo.

Se añadieron doce entradas al final de `InputManager.asset`: D-Pad horizontal/vertical, turbo, pausa y ocho ejes sin dead zone para diagnóstico. **Los bytes completos del Input Manager anterior permanecen como prefijo**, incluidos `Horizontal`, `Vertical`, `Submit`, `Cancel` y los sticks originales. No se sustituyó el archivo por el de Halo 2D.

La referencia pública [Input Manager de Halo 2D](https://github.com/VegettoSan/Halo2D_PsVita/blob/main/Halo%202D/ProjectSettings/InputManager.asset) se consultó junto con [sus limitaciones documentadas](HALO2D_PSP2_REFERENCE.md). SHA-256 de la referencia leída: `da4a89bdf24cfaced57dea0fcfa6813f4f1e6dbc77cb17e780cb06d5d22e0962`.

| Control propuesto | Ruta lógica | Configuración candidata, UNTESTED en Vita |
| --- | --- | --- |
| Stick izquierdo | Entrada analógica original de `GameInput` | Ejes 0/1 existentes; dead zone 0,19; inversión vertical existente |
| D-Pad | Alternativa digital; navegación FGOL | Izquierda/derecha 11/9; abajo/arriba 10/8, tomados como candidatos de la referencia |
| Cross | Acción A / Submit | `Submit` existente, candidato joystick 0 |
| Circle | Regreso por `GUIManager.OnHardwareBackButton` | `Cancel` existente, candidato joystick 1 |
| R mantenido | Acción original de turbo | Alias configurable, candidato joystick 5 |
| START | Pausa/reanudación original | Alias configurable, candidato joystick 7 |
| Square / Triangle | Acciones X/Y originales | Candidatos 2/3; HUD primario: misiones; secundario: mapa, sujetos a disponibilidad original |
| L | Reservado | Alias candidato 4, sin nueva mecánica |
| Stick derecho | Sin asignación nueva | Pendiente de función y calibración apropiadas |
| Pantalla táctil | Ruta original | Prueba física PSP2 pendiente |

**UNTESTED:** todos los números físicos de esta tabla. Una configuración presente en otro proyecto no demuestra su distribución en este juego/player/dispositivo. El perfil y los aliases permiten corregirla tras medirla.

El stick y el D-Pad no se suman para mover al tiburón; el stick tiene prioridad cuando entrega entrada. Las pulsaciones táctiles conservan su ruta. Las pantallas que bloquean gameplay entregan cero movimiento y rechazan el turbo del mando.

## NGUI

**VERIFIED en pruebas de Editor:** se reutilizan los enlaces y mensajes FGOL existentes, incluidos los botones situados en anclajes compartidos. La tienda construye sus enlaces originales entre artículos y conserva los mensajes táctiles; el mando utiliza su destino de compra original. No se alteran precios, inventario ni reglas de compra.

El adaptador aporta foco inicial, memoria del foco al regresar de ventanas, validación de elementos visibles/habilitados, señal visual y repetición cada 0,2 segundos de tiempo real, también en pausa. Primero ejecuta los enlaces originales; cuando un botón carece de enlace, prueba `OnKey`/`UIButtonKeys` y busca un vecino visible. Los scrollers que mantienen selección mientras cambian contenido conservan ese contrato. La confirmación sin configuración FGOL utiliza el press/release de NGUI.

La pausa original tiene contenido `OptionsMenu` hermano del wrapper `Popup`; sus candidatos incluyen ambos sin reparentar objetos. Los contenedores sin collider no se consideran botones; los scrollers originales siguen siendo seleccionables. No se editaron escenas, prefabs ni metas existentes.

## Verificación y límites

**VERIFIED:** compilación del ensamblado principal con Mono/mcs instalado en Unity 2018.2.19f1, C# 4: cero errores, 262 advertencias preexistentes. También pasa una compilación de fuente con símbolos PSP2 y sin `UNITY_EDITOR`, usando las bibliotecas instaladas. Esta comprobación no construye ni ejecuta un player PSP2.

Pruebas controladas en la instancia existente del Editor, con autorización del propietario:

- Entrada lógica analógica `(0,4; 0,2)` recibida sin normalizar su intensidad; movimiento observado.
- Turbo mantenido: activación, consumo, agotamiento y recuperación tras liberar, por la lógica original.
- START pausa/reanuda; durante pausa, stick y turbo solicitados producen `(0,0)`, `tap=false`, `held=false`, sin impulso.
- Menú principal, evolución, cambio de tiburón, mejoras y confirmación/cancelación; compra real de una mejora con 50 monedas ficticias, conservando su cálculo original.
- Tienda: navegación entre artículos, apertura/cancelación de diálogo, regreso; foco restaurado tras ventanas emergentes.
- Enter y Escape reales como Submit/Cancel; navegación a salida de partida y transición original a resultados.
- Resultados: foco visible en cerrar, confirmación y regreso a selección. Contrato nativo de selección: rechaza widget deshabilitado, alpha cero, collider deshabilitado y objeto inactivo.

**UNTESTED:** mando Windows físico (no se detectó joystick), botones y touch de Vita, todas las combinaciones de inventario/desbloqueos, todas las pantallas de niveles/mapas, muerte/reintento completos, desplazamientos extensos y toda variante de diálogo. La prueba inyecta acciones lógicas solo en Editor; no valida índices físicos ni constituye una certificación de navegación completa.

Los intentos fallidos se preservaron: un mapper duplicado destruía `King`; un foco elegido durante la apertura seleccionaba controles transitorios; el contenido de pausa y los anclajes compartidos quedaban fuera del ámbito inicial. Se corrigieron esas causas y se repitieron sus casos. No se modificaron escenas para evitar los fallos.

**VERIFIED, integridad:** los 19.591 archivos inventariados de Assets/ProjectSettings de Unity 5.6 mantienen sus hashes. La intervención registra 24 archivos existentes modificados y cinco nuevos, con sus metas nuevas; el probe temporal fue retirado. Durante el intervalo también cambiaron cinco metas de importadores de textura ajenos a controles; se documentaron por separado y no se revirtieron ni se atribuyen a esta implementación. Sus GUID se conservan. Los tres archivos de progreso local respaldados antes de las pruebas se restauraron byte por byte; se conserva la partida posterior de prueba en privado.

La captura posterior al último grupo conserva cero excepciones C# y dos avisos de limpieza al abandonar Play. Las observaciones anteriores de animación se mantienen como pendientes independientes. El Editor quedó abierto, fuera de Play, con su escena anterior restaurada, preview desactivado y diagnóstico desactivado.

**VERIFIED:** se observaron avisos adicionales de animaciones heredadas (`fly` y `paddle`) y limpieza de objetos al salir de Play. Se registran por separado; no se modifican IA ni efectos para silenciarlos. No se afirma una consola completamente libre de errores ni paridad total de gameplay.

## Siguiente prueba

1. Editor: activar el preview antes de Play y arrancar desde `SplashLoaderScene`, no desde una escena intermedia. Repetir selección, confirmación, cancelación, mejoras, tienda, pausa y regreso de ventanas con el inventario habitual. Desactivar el preview después.
2. Para una prueba PSP2 autorizada, habilitar temporalmente `HSE_VITA_INPUT_DIAGNOSTICS`. El diagnóstico estándar muestra ocho ejes crudos, veinte botones, signos, estados mantenido/down/up y dirección mapeada. Medir centro y recorrido de cada stick y pulsar cada botón por separado.
3. Ajustar solo el perfil/aliases con los resultados medidos; repetir movimiento proporcional, liberación de R, START, D-Pad, foco, touch y ventanas. Retirar el símbolo de diagnóstico al terminar.

No se generó VPK, no se publican recursos propietarios y no se modifican las aprobaciones formales.
