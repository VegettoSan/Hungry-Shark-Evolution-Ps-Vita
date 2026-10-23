# Preparación v1.0 — 2026-10-23 UTC

**Reconstrucción local PASS; matriz Vita PASS por reporte del propietario; publicación BLOCKED por rutas privadas en la base.**
Excepción específica solicitada por VegettoSan para distribución en `main` existente,
incluida su selección explícita del VPK de referencia. No aprueba P12: P03 sigue
ACTIVE, P00–P02 APPROVED, P04–P12 LOCKED. No se abrió Unity, se recompiló el juego
ni se escribieron sus proyectos o VPK original. Todo binario producido es privado.

## Entradas VERIFIED

| Entrada | Bytes | SHA-256 |
| --- | ---: | --- |
| APK 6.4.2 / build 228 | 104296014 | `52b7d8754ab8fec75c3b656a0dc5ec8dc849565adc402487a3ac093f6a481f7a` |
| `HSE_BetterWinds_v2_PresentationOnly_HGSKE1178.vpk`, designado por el propietario | 136185607 | `3e3dffc4a175f8e11ca769c1f0cec69d5cb79b1095fcbccf312f9a4c58df9db3` |
| ZIP del prototipo suministrado | 10844 | `9998f1259df71e005a7cb023aa998565a602a766221039f6bf11afcc94779210` |

CRC APK/VPK PASS. SFO real: `HGSKE1178`, `Hungry Shark Evolution Vita`,
APP_VER/VERSION `01.00`, categoría `gd`. Coincide con la
[evidencia BetterWinds v2](BETTERWINDS_LIVEAREA_V2_INTEGRATION_2026-10-23.md).
La procedencia Unity 2018.2.19f1 PSP2 se sustenta en ese registro previo;
no se ejecutó otro build ni se certificaron licencias del toolchain.

## Inventario real y destino

214 archivos / **380836805 bytes descomprimidos**:

| Grupo observado | Archivos | Bytes | Tratamiento privado experimental |
| --- | ---: | ---: | --- |
| `Media/Managed/` | 140 | 16665854 | Base: ensamblados del juego, Unity/Mono/terceros y símbolos |
| `Media/Modules/` | 4 | 79262268 | Base: módulos PSP2/Mono, incluido `MonoAssembliesPSP2.suprx` |
| Resto de `Media/` | 57 | 241141828 | 54 entradas serializadas/sidecars al payload; dos recursos Unity y boot.config en base |
| `eboot.bin` | 1 | 40017228 | Base: ejecutable intacto |
| `sce_module/` | 5 | 3131088 | Base: módulos nativos |
| `sce_sys/` | 7 | 618539 | Base: SFO, presentación y manual; crédito BetterWinds conservado |

**VERIFIED:** payload = 54 entradas /237636124 bytes; base = 160 entradas
/143200681 bytes descomprimidos. Se preservan todos los archivos y rutas internas.
El inventario, SHA por archivo, relaciones APK, `data_paths.txt` y clasificación
de las 214 entradas permanecen privados en el directorio ignorado
`local-work/Release_20261023/`: `final_audit_relationships.json` y
`disposition.private.json`. Base/payload no es clasificación legal. La base
incompleta no se ha probado como app independiente: sólo se instala el VPK completo.
`Media/` es un hecho del ZIP; no mide los valores de las APIs Unity de rutas ni
la ubicación física del guardado.

**VERIFIED:** cero archivos completos del VPK coinciden por SHA-256 con entradas
del APK, incluso después de unir los 15 grupos fragmentados. La extracción directa
sin transformación no reconstruye esos archivos. Esto no significa cero
coincidencias parciales. No se reemplazaron datos PSP2 con homónimos Android.

## Comparación medida

Mismo payload de 54 archivos, encoder y decoder Windows reales. Opciones
`-S none -D -A -B 536870912` al codificar; `-d -D -R -B 536870912` al decodificar.
Hashes y comparación byte por byte de 214 entradas PASS en las tres variantes.

| Fuente / ZIP payload | ZIP bytes | Parche bytes | Parche en ZIP nivel9 | Autoría + roundtrip + comparación |
| --- | ---: | ---: | ---: | ---: |
| APK / deflated | 94484202 | 94273281 | 94010046 | 25.769 s |
| APK / stored | 237643804 | 110139062 | 92385286 | 30.784 s |
| Normalizada / stored | 237643804 | 93302025 | 77293827 | 26.743 s |

Base comprimida: 41705984 bytes en las tres variantes. Tiempos observados en este
equipo, no promesas para otros Windows.

La fuente `unity-data-v1` concatena los archivos lógicos de `assets/bin/Data/`
en orden ordinal de ruta, uniendo `.splitN` numéricamente, sin cabeceras,
delimitadores ni transformación del juego. Resultado 311224301 bytes, SHA-256
`1dfde9a226099add77023c0ff4259b42cc82767b8459f597ccdc2d726abed12b`.
Python y C# regeneran independientemente el mismo hash desde el APK exacto.
El usuario no necesita Python, Unity o SDK.

Contabilidad VCDIFF independiente, contrastada con todas las ventanas y la
salida completa del inspector que falla al finalizar:

| Variante | COPY fuente | COPY target | ADD literales | Bytes emitidos por RUN |
| --- | ---: | ---: | ---: | ---: |
| APK / deflated | 28093 | 721950 | 93727395 | 6764 |
| APK / stored | 2466493 | 154536477 | 70937861 | 9702973 |
| Normalizada / stored | 51558156 | 116129342 | 60405613 | 9550693 |

Cada fila suma exactamente el ZIP payload. COPY cuenta bytes emitidos, con
repeticiones posibles, no bytes únicos ni porcentajes de propiedad. ADD incluye
cabeceras y otros bytes; no clasifica automáticamente recursos protegidos. COPY
target puede repetir contenido introducido por ADD. RUN almacena una semilla por
instrucción, no literalmente todos los bytes emitidos. Reducir tamaño no acredita
redistribución permitida.

**Decisión experimental:** variante normalizada para revisión privada por mayor
reutilización desde el APK y menor transporte. **Ningún mecanismo público está
autorizado.** Un conversor determinista que regenere todo el PSP2 sin parche
sigue NOT TESTED; no se inventó uno.

## Herramienta fijada y fallos conservados

[xdelta3 3.2.2 oficial](https://github.com/jmacd/xdelta/releases/tag/v3.2.2).
ZIP upstream 188145 bytes, SHA-256
`d48f7651d86b78fdca397057b46601f89131f37219675df736db70af44ec3379`.
Se descargó la misma versión sólo para comprobar procedencia; los dos ejecutables
locales coinciden byte por byte con el ZIP y su digest oficial publicado por GitHub.

| Binario | SHA-256 |
| --- | --- |
| `xdelta3.exe` | `732980d52f9220dc81817ccdd0c295eadce8a0048a24c2a771a73f791ac824e9` |
| `xdelta3decode.exe` | `67f84255bd009a65b1252784f8b3ebc72b5907766bc366c571e8b85fa7002a06` |

**FAIL conservados:** LZMA por defecto devuelve `unavailable secondary compressor`;
DJW codifica una muestra, pero el decoder separado la rechaza; FGK no está compilado.
`printdelta` emite todas las ventanas y sale con `0xC0000005`, incluso en muestra
pequeña. No se declaró exitoso ese proceso. `inspect_delta.py`, lector limitado
basado en [RFC3284](https://www.rfc-editor.org/rfc/rfc3284), cuenta VCDIFF v0/default
table sin compresión secundaria y rechaza formatos no soportados. Coincide en las
tres variantes y una muestra sintética conocida. No reemplaza el decoder real,
que sí pasa con `-S none`.

Se preservan además primer error de comillas C#, ArrayList del deserializador,
comando csc con rutas relativas, lanzamiento prematuro del bundle y TEMP del
sandbox sin permisos. Herramientas/comandos corregidos; juego intacto. Las cinco
pruebas Python pasan usando TEMP privado escribible.

## Windows y fidelidad

**PASS VERIFIED:** extractor C#/.NET Framework x64 sin Python, progreso en siete
etapas, mensajes ES/EN, SHA de entrada/componentes/decoder/fuente/payload/cada
archivo final, límites, espacio libre y no sobrescritura. Temporal único junto al
destino; Ctrl+C termina y espera sólo su hijo antes de limpiar. `File.Move` publica
localmente sin reemplazo, en el mismo volumen.

Dos comparadores independientes Python/C# confirman **214 archivos idénticos,
byte por byte**. Reconstrucción repetida en ruta con ñ y caracteres japoneses:
mismo SHA del contenedor. Una repetición: 17.16 s, pico working set extractor
43458560 bytes (**excluye decoder hijo**). Memoria total del árbol y pico de disco
NOT TESTED; cálculo de espacio conservador. Host Windows NT10.0.26200 x64.
Windows10/PC limpio y recorrido manual del selector/drag-drop: NOT TESTED.
Interrupción Ctrl+C simulada durante decoder PASS; corte eléctrico/cierre forzado
NOT TESTED. Pueden dejar `.hse-rebuild-*` con `HSE_TEMP.txt`; sólo retirar el temporal
identificado después de confirmar que terminó la herramienta.

**PASS:** 32 controles nativos de rutas, colisiones, fragmentos, comparación,
APK incorrecto por tamaño y hash, falta de espacio, componentes ausentes/dañados,
decoder no verificado, manifiesto incorrecto, checksum decodificado, cancelación,
no sobrescritura y ausencia de salidas parciales. Cinco pruebas Python PASS.
[Comandos reproducibles](../tools/release_xdelta/README.md).

## Hardware y derechos

El propietario primero reportó una prueba parcial sin problemas y después confirmó:
«ya revise y probe todo lo que me dijiste y si paso todo en orden, vpk validado».
**OWNER-REPORTED PASS:** instalación, arranque, sonido, menús, evolución,
tiburones especiales, gameplay, pausa, guardado/carga, reinicio en frío,
actualización y reinstalación. Se acepta su confirmación de toda la matriz solicitada;
no se pide repetirla. No se aportaron logs, repeticiones cuantificadas o hash
instalado; Codex no observó la consola. No es aprobación formal de fases.

Sobre permisos, el propietario declaró: «si, los tengo, todos los documentos en
regla», en respuesta a la pregunta que enumeró código/recursos HSE,
player/módulos Unity–Sony y BetterWinds. **OWNER-ATTESTED:** afirma disponer de los
permisos. No se proporcionaron referencias ni se inspeccionaron los documentos;
no se presenta esa declaración como una certificación jurídica independiente.
El crédito [BetterWinds](https://github.com/BetterWinds) se conserva.

La [licencia xdelta3 exacta](https://github.com/jmacd/xdelta/blob/v3.2.2/xdelta3/LICENSE)
es Apache-2.0, guardada en el expediente/bundle privado. No concede derechos sobre
entradas/resultados. Revisión de licencias/avisos de dependencias enlazadas:
pendiente antes de distribuir binarios. Los
[términos Unity actuales](https://unity.com/legal/editor-terms-of-service/software)
describen derechos condicionados; no acreditan por sí solos las licencias
históricas/console aplicables a este build. Acceso autorizado de plataforma:
[PlayStation Partners](https://partners.playstation.net/). No se emitió una
certificación jurídica sobre el port.

**Revisión de privacidad VERIFIED:** el escaneo detecta dos cadenas de ruta
absoluta de la carpeta privada de compilación dentro de
`Media/Modules/MonoAssembliesPSP2.suprx`. El valor no se publica en este informe.
También se detecta una ruta de compilación del SDK Facebook en su DLL de settings.
Las seis cabeceras PEM de eboot contienen cuatro bloques completos de clave:
los cuatro coinciden por SHA-256 del DER con las claves de prueba públicas de
[Mbed TLS 2.6.0, certs.c](https://github.com/Mbed-TLS/mbedtls/blob/mbedtls-2.6.0/library/certs.c).
No se etiquetan esas muestras conocidas como credenciales privadas del propietario.
No hubo coincidencias de tokens GitHub/AWS o claves personales Windows en los
patrones probados; esto no certifica ausencia de todo posible secreto.

**Impedimento concreto actual:** publicar la base tal como está expondría las
rutas privadas que el propietario pidió excluir. No se reescribió el módulo
nativo ni se ocultaron las rutas en otro parche. Una referencia limpia, aprobada
y validada, o una decisión explícita del propietario sobre este hallazgo, es
necesaria para resolver esa condición. El original y la reconstrucción validada
siguen byte por byte intactos. El permiso de redistribución declarado no se
interpretó como permiso para revelar rutas privadas. Los 60405613 bytes ADD siguen
documentados como contenido real del parche, no como una exención de derechos.

## Artefactos y GitHub

Binarios sólo en `local-work/Release_20261023/`, ignorado por Git.
`HSE_v1.0_PRIVATE_REVIEW/` contiene extractor, decoder, base, parche, manifiesto,
licencia, instrucciones, aviso privado y checksums. Tamaños/hashes medidos en
[recibo sanitizado](evidence/RELEASE_PREPARATION_2026-10-23.json).
Son **candidatos privados**, no assets de una Release publicada.

**VERIFIED:** `gh release list --repo VegettoSan/Hungry-Shark-Evolution-Ps-Vita
--json tagName,isDraft,isPrerelease,publishedAt` devolvió `[]` el 2026-10-23.
Sin tag, rama nueva, draft Release, Release pública ni uploads. La herramienta
GitHub conectada no ofrece gestión de Releases; se usó CLI para consulta read-only.

[Guía ES/EN](RELEASE_V1_0_USER_GUIDE.md) preparada como borrador. Antes de adjuntar
únicamente assets autorizados a una Release `v1.0`: resolver las rutas privadas,
completar los avisos de terceros y la revisión documental aplicable, y conservar
la evidencia de pruebas Windows y la matriz Vita confirmada por el propietario. El APK y VPK completo original no están autorizados
para publicación. Ledger sin modificaciones.
