Todo:

- [x] Add intent abstraction for rendering commands.
- [x] Split GLWrapper functions into a new renderer class + intents.
- [ ] Internalise TexureGL.
- [ ] Create an internal FrameBufferGL, expose FrameBuffer as the platform-independent storage.
- [ ] Remove the ability to create FrameBuffers.
- [ ] Add intent abstraction for vertex additions.
- [ ] Remove the ability to create and bind vertex buffers/batches + internalise. Rename to VertexBufferGL, etc.
- [ ] Remove vetexAction from DrawNode.Draw().
- [ ] Give DrawNode.Draw() an IRenderer parameter instead.
- [ ] Remove Push/Pop duo, use either use IDisposable or state management (e.g. SaveState()).
- [ ] Add enter/exit intents to cover the entire DrawNode.Draw().
- [ ] Remove GLWrapper, make things non-static.
