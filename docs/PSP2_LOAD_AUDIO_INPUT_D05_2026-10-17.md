# D05: carga de zona, audio y foco del slider

**Actualización posterior — VERIFIED por informe del propietario:** D05 ya permite jugar, carga gameplay y reproduce sonidos; informa 40–58 FPS. Solicita elevar contenido y nitidez porque observa escenas low y texturas borrosas. Véase [la prueba D06](PSP2_MEDIUM_D06_2026-10-17.md). Esto actualiza las comprobaciones pendientes de carga/audio/FPS descritas al preparar D05; no confirma todas las zonas, controles o estabilidad prolongada.

Encargo expreso del propietario tras probar D04. Evidencia de 2026-10-17 UTC; capturas privadas del 16 de octubre local. P00 sigue ACTIVE, sin aprobaciones de fase.

## Evidencia de hardware

**VERIFIED por capturas:** D04 completa `14 LaunchFrontEnd`, `15 Init completed`, pasa por Empty y carga Master_low. Aparece `Cannot create FMOD::Sound instance for resource resources.resource, (Unsupported file or audio format.)`. El propietario informa que el slider de edad no admite navegación, el foco de Aceptar está debajo de su imagen, hay crash al cargar y 10–15 FPS. La captura de carga indica Update/s 1,3: no es un benchmark de gameplay estable ni GPU frame time. Se superó el bloqueo Facebook de D03; no se declara jugable el port.

## Crash demostrado

Dump privado: 1.989.968 bytes, SHA-256 **`9e79ce3ad26830649237ccefc4350d4548d0a725afa054d0d7b69399124ababa`**, gzip de ELF core ARM de 24.254.080 bytes. Layout de notas contrastado con el [código primario vita-parse-core](https://github.com/xyzz/vita-parse-core/blob/master/core.py). Análisis con los símbolos SymbolFiles/D04_Player.self del build entregado, unfself y binutils VitaSDK ya instalados en WSL; sin descargar herramientas ni subir el dump.

**VERIFIED:** hilo UnityGfxDeviceWorker, Data abort `0x30004`; PC **0x818708dc**, módulo UnityPlayer_2018219_M, offset **0x8708dc**, función **BufferGXM::UpdateInternal(unsigned int, void const*)**. La instrucción lee `[r0+16]` con **r0=0**. Candidatos de pila resuelven a GfxDeviceGXM.InitializeBufferInternal / GfxDeviceWorker.RunCommand; no se presentan como un backtrace completo con unwind. Addr2line conserva un aviso DWARF y no ofrece líneas C++ fiables.

**VERIFIED:** la rama de resultado cero de GfxDeviceGXM.Alloc libera el bloque y vuelve a la ruta de desreferencia nula. La TTY registra reserva física fallida de **0x00100000** en ScePhyMemPartGame y disponibles **0x00025000 / 0x00024000**. Esos mensajes aparecen durante el manejo del crash y no bastarían solos para atribuirlo: los registros y la rama de asignación fallida completan la evidencia. El fallo inmediato es un buffer gráfico sin memoria; no una pila FMOD. No se parchea el player propietario.

## Audio y presupuesto de memoria

**VERIFIED en D04:** 305 clips, **302 Vorbis y tres PCM**. La implementación instalada de UnityEditor.AudioImporterInspector.GetFormatsForPlatform, PSP2=18, excluye Vorbis y admite HEVAG; el módulo incluye vagconv2. No se extrapola una opción de Unity moderno.

[HSEPSP2D05Audit.cs](../tools/unity/HSEPSP2D05Audit.cs) añade overrides **solo PSP2** de los 302 Vorbis a **HEVAG**, con respaldo de cada meta. **PASS importación:** formato objetivo HEVAG y mismos canales, frecuencia y muestras; modo de carga/streaming preservado. Los tres PCM y las opciones de otros destinos quedan intactos. No se convierten todos los sonidos a PCM residente ni se cambia AudioManager/volumen/mute. El fallo de formato observado ahora no invalida la evidencia histórica independiente de opciones internas silenciadas.

**VERIFIED baseline:** psp2MemoryExpansionMode=0. El manual instalado **PSVitaTipsForMaximizingAvailableMemory.html**, apartado Memory Expansion Mode, permite 29/77/109 MiB adicionales. La enum real de este editor confirma ExpandBy109MB=3. D05 elige **ExpandBy109MB** mediante PlayerSettings.PSVita para dar margen a la reserva física fallida conservando contenido. **Coste documentado:** navegador y tienda del sistema no disponibles mientras la app está activa. No amplía CDRAM ni garantiza que toda zona futura quepa. Threading, calidad, shaders, geometría y horneado se conservan.

## Navegación y recuadro

**VERIFIED estático:** UBMSlider.Update ya ajusta el valor y emite OnSlideChanged al recibir izquierda/derecha, si selectedObject es exactamente m_dragger. Candidates lo excluía al no reconocer un handle sin los componentes habituales de botón/configuración, y el fallback horizontal podía quitarle foco. Se incluye el handle y se consume la navegación horizontal mientras está seleccionado para que actúe UBMSlider original. Arriba/abajo conserva la navegación normal.

El recuadro usaba Collider.bounds, que puede estar desplazado respecto de la imagen. [HSEVitaWidgetGeometry.cs](../tools/unity/HSEVitaWidgetGeometry.cs) calcula el rectángulo con tamaño, pivote y transform de UIWidget visibles según la convención NGUI; foco y selección direccional usan esa geometría. Los controles sin widgets conservan su fallback. No se mueven colliders, prefabs o elementos de UI; no se cambia Popup_AgeGate ni la lógica de edad/aceptación.

**PASS fixture nativo de editor:** handle reconocido y presente en Candidates, borde sobre el widget con collider deliberadamente desplazado. No instancia ni acepta un diálogo de edad. Su primera compilación intentó asignar mainTexture, propiedad de solo lectura de este NGUI: CS0200 preservado, corregido antes de ejecutar cambios de audio. Mando/táctil reales aún requieren prueba en Vita.

## Rendimiento y verificación pendiente

Se conserva el límite solicitado de 60 FPS. Los **10–15 FPS siguen abiertos**: falta separar carga, memoria, CPU y GPU en una sesión estable. D05 añade cuatro muestras por escena de memoria libre LPDDR/CDRAM/PHYSCONT y memoria asignada Unity, con la última visible en la franja; registra unos pocos AudioSource/clip/loadState/volúmenes. Update/s no equivale a presentaciones físicas. La conversión/margen podrían ayudar, pero no se afirma ganancia de FPS. No se recortan recursos ni se modifica el timestep o reloj para mejorar un contador.

## Respaldo, artefacto y reversión

**VERIFIED build PSP2 Mono2x:** Succeeded, **0 errores / 294 advertencias**, 2 min 4,31 s. Mismas 18 escenas originales. Tamaño del informe 246.770.628 bytes: no equivale a memoria residente.

**VERIFIED player:** 305 clips serializados, **302 HEVAG / tres PCM**, sin Vorbis; la conversión llega al player. El SFO cambia ATTRIBUTE2 de **0x0 en D04 a 0xc en D05**, coherente con el ajuste de memoria del módulo. El contrato IL de exclusión SDK vuelve a pasar: no runtimes Facebook ni construcción de proveedores Android y guardado local original presente.

**VERIFIED paquete privado:** `Reports/LoadAudioInput_20261017/Build/HSE_LoadAudioInput_D05_60fps_HGSKE1178.vpk`, **134.945.356 bytes**, SHA-256 **`ad458ba0799ce2fcc79e1c033c7d2d6c3dba2067a07d95ad74a5ab553675a972`**. CRC y comparación de cada miembro runtime con el player pasan; solo SFO cambia por identidad/etiqueta. El aumento del tamaño frente a D04 refleja el nuevo audio empaquetado, no una medida de RAM.

Evidencia privada en Reports/LoadAudioInput_20261017: entradas, BeforeManifest/Before, NavigationChange, AudioMemoryResult, InputGeometryContract, ScopeChecks, BuildResult, D05_Audio, NativeSDKContract, PackageReceipt, fault/notes e IntegrityAfter. **VERIFIED:** 19.617 archivos iniciales comprobados; solo 302 metas de audio, el adaptador de navegación y un campo de ProjectSettings cambiaron. GUID/default settings/mono/normalización/precarga/background de audio preservados; ningún archivo original ausente. Único añadido permanente: helper de geometría/meta. No cambian escenas, prefabs, fuentes de audio, texturas, mallas o iluminación. Los 19.591 archivos de la referencia 5.6 siguen idénticos, sin añadidos. D04 se conserva.

Revertir D05: restaurar HSEVitaNavigation.cs, metas de audio afectados y ProjectSettings.asset desde Before; retirar helper de geometría/meta. Conservar correcciones anteriores de caché/shaders, SDK y FPS. **Marcadores, sonda, constructor y auditor temporales retirados de Assets después de empaquetar**; King.cs restaurado exactamente. Siguen en DiagnosticFiles para reproducir D05. El VPK de prueba contiene la sonda; no es una versión final sin diagnóstico.

**UNTESTED en hardware:** efecto de ampliación sobre la carga, desaparición del error FMOD/audio audible, navegación/contorno físicos, FPS y estabilidad. Diagnóstico del crash y compatibilidad del formato demostrados; correcciones compiladas y empaquetadas, pendientes de confirmación en consola.

Prueba física: instalar D05 sobre HGSKE1178 sin borrar saves. Si pide edad, probar manualmente el slider y Aceptar con mando/táctil, sin resetear preferencias. Comprobar menú, sonido con opciones originales habilitadas y la misma carga que falló. Registrar memoria/FPS tras terminar carga y probar movimiento, boost, pausa/regreso. Si falla, conservar el dump nuevo: el anterior corresponde a D04. **No se declara resuelto por el build.**
