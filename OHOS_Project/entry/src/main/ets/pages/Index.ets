@Entry
@Component
struct Index {
  @State message: string = 'Hello World';

  build() {
    Row() {
      XComponent({ type: XComponentType.SURFACE, id: 'AvaloniaRootComponent', libraryname: "entry" })
    }
    .height('100%')
  }
}
