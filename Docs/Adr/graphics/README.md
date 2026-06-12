[ADR Home](../README.md)

# graphics

This category describes the executable graphics runtime: OpenTK, GPU, shaders, buffers, atlases, and input.

## Scope

Look here for render execution, shader assets, GPU contracts, windows, input controllers, and OpenGL runtime behavior.

## Responsibility Boundaries

Graphics does not model domain data or logical scene behavior. It executes decisions that were already made in `rendering/`.

## How To Start Reading

Start here when changing shaders, GPU buffers, runtime windows, or low level rendering execution.

## ADR List

| ADR | Title |
| --- | --- |
| [068-share-compute-pipeline-bases-for-gpu-culling-and-texture-generation.md](./068-share-compute-pipeline-bases-for-gpu-culling-and-texture-generation.md) | Share Compute Pipeline Bases For GPU Culling And Texture Generation |
| [069-use-gpu-driven-rendering-for-dense-visuals.md](./069-use-gpu-driven-rendering-for-dense-visuals.md) | Use GPU Driven Rendering For Dense Visuals |
| [076-build-specialized-buffer-wrappers-on-top-of-a-generic-gpu-buffer.md](./076-build-specialized-buffer-wrappers-on-top-of-a-generic-gpu-buffer.md) | Build Specialized Buffer Wrappers On Top Of A Generic GPU Buffer |
| [077-keep-buffer-lifecycle-and-binding-logic-out-of-renderers-and-pipelines.md](./077-keep-buffer-lifecycle-and-binding-logic-out-of-renderers-and-pipelines.md) | Keep Buffer Lifecycle And Binding Logic Out Of Renderers And Pipelines |
| [078-use-typed-gpu-buffer-contracts.md](./078-use-typed-gpu-buffer-contracts.md) | Use Typed GPU Buffer Contracts |
| [079-provide-runtime-fallbacks-for-text-and-icon-assets.md](./079-provide-runtime-fallbacks-for-text-and-icon-assets.md) | Provide Runtime Fallbacks For Text And Icon Assets |
| [080-render-labels-and-icons-through-atlas-assets.md](./080-render-labels-and-icons-through-atlas-assets.md) | Render Labels And Icons Through Atlas Assets |
| [081-keep-shader-sources-as-validated-assets.md](./081-keep-shader-sources-as-validated-assets.md) | Keep Shader Sources As Validated Assets |
| [082-register-runtime-shaders-through-a-static-manifest.md](./082-register-runtime-shaders-through-a-static-manifest.md) | Register Runtime Shaders Through A Static Manifest |
| [083-render-derived-effects-into-dedicated-gpu-targets.md](./083-render-derived-effects-into-dedicated-gpu-targets.md) | Render Derived Effects Into Dedicated GPU Targets |
| [084-execute-graphics-runtime-through-an-ordered-frame-graph.md](./084-execute-graphics-runtime-through-an-ordered-frame-graph.md) | Execute Graphics Runtime Through An Ordered Frame Graph |
| [085-keep-opentk-render-output-as-a-thin-runtime-adapter.md](./085-keep-opentk-render-output-as-a-thin-runtime-adapter.md) | Keep OpenTK Render Output As A Thin Runtime Adapter |
| [086-use-opentk-as-the-windowing-and-opengl-runtime.md](./086-use-opentk-as-the-windowing-and-opengl-runtime.md) | Use OpenTK As The Windowing And OpenGL Runtime |
| [087-keep-input-controllers-outside-rendering-state.md](./087-keep-input-controllers-outside-rendering-state.md) | Keep Input Controllers Outside Rendering State |
| [088-use-debug-windows-for-rendering-development.md](./088-use-debug-windows-for-rendering-development.md) | Use Debug Windows For Rendering Development |
