using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AOOH_Gallery.Views;

public partial class FQA : UserControl
{
    public static string FQA_1 =
        """
        我们正在寻找一个包含丰富 Avalonia 基础内容的控件库用于展示，Semi 和 Ursa 恰好能够满足我们的要求。
        以下是作者对它们的介绍：
        
        Hello大家好，我是Avalonia中文社区的发起人和主理人董彬。

        今天通过这次简单的闪电演讲简单介绍一下我们的开源项目大熊Ursa以及其中所包含的设计理念。

        关于Avalonia我这边就不过多介绍了，前面孙策老师的session大家应该都听到了。

        在今年我们的开源贡献主要是聚焦在Semi和Ursa两个项目中。想要了解Ursa的设计理念，我首先要谈一谈Semi的设计理念。Semi是我们的一个样式库，他为Avalonia的所有内置控件提供了基于字节跳动Semi Design风格的主题。Semi Avalonia是一个非常纯粹的样式库，他只有样式，没有功能。

        Semi在设计之初就是针对Avalonia V11的新世界打造，充分利用了ControlTheme+Style结合的样式体系，于是我们不需要derive一个子类增加关于样式的属性，也不需要声明很多附加属性，单单凭借StyleClass，就可以让控件的外观拥有更多的可能性。

        Semi这个样式库在理论上可以实现的另一个原因是Avalonia控件充分支持无样式设计。Avalonia的原生控件的功能与样式是完全分离的，只要控件的模板内包含必须的功能相关的元素，那么他在哪里，如何布局，用什么类型去实现，都不重要。

        那么我一方面出于探索Avalonia样式系统的目的，另一方面考虑到公司的整体产品规划，Semi Avalonia主要遵循了以下几个设计理念：
        第一，Semi Avalonia完全不引入自定义控件，所有的视觉风格全部都使用Avalonia内置的元素实现，尤其是不会使用内部类。这主要是为了方便开发者基于Semi Avalonia进行二次开发。如果你需要在自己的项目中开发一个自定义控件，那么Semi Avalonia的所有视觉效果都应该可以直接仿照实现。这是我们和Material、FluentAvalonia等项目最大的区别。我们始终承诺，一个程序可以无缝地在内置Fluent主题和Semi Avalonia之间切换，他运行起来可能看起来不太舒服，但绝对不会跑不起来。虽然我们是Semi的开发维护者，虽然我们非常希望大家更多地去使用Semi，但我们设计这个项目最先考虑的问题，就是当大家不想用Semi主题的时候，如何帮助大家潇洒地转身离开。
        第二，Semi Avalonia采用了非常细粒度的DesignToken。Design Token是一个在UI设计界比较常见的概念。比如我的设计体系中有基础的色板提供颜色的Token，比如Blue12345，Red12345，然后我可能会有一层业务相关的抽象token，比如我的Primary，我的Warning。最终我可能会有一层代码相关的token，比如我的ButtonHoverBackground，TextForeground等等。在Semi Avalonia中我们暴论给控件的是最细粒度的Token。我们采取这样的设计，主要是为了实现UI最细粒度的资源变更。在Semi Avalonia中，你可以在UI的任何一个节点通过重新定义动态资源的方式对控件中引用的动态资源进行覆盖。完全不影响视觉树中的其他元素。并且由于我们前期对DesignToken做了非常正确的规划，因此我们几乎没有对控件模板的基础施舍做修改，就顺利地引入了高对比度配色的四种配色方案，这也证明了Semi Avalonia在配色方面的灵活性

        Semi Avalonia本身是一次非常成功的尝试，但这种设计理念导致我们没有办法在功能上对Avalonia进行拓展，于是我们就开启了第二个开源项目Ursa。Ursa本身在定位上可以说是与与Semi Avalonia完全相反的。Semi只有样式，没有功能，而Ursa只有功能，没有样式。他完全遵守Avalonia控件无样式设计的理念，只在代码中定义最核心的与功能相关的模板子元素，而完全不在代码包含任何与样式相关的内容。

        Ursa在设计的时候还进行了以下几个方面的考量，也算是Ursa的设计理念。这里要提前说明的是，Ursa的这些所谓的设计理念，是从控件库设计的角度考虑，和真正的应用开发并不一样。
        首先，Ursa整体上是视图模型中立的。一方面呢，Ursa中的控件通常并不规定用户的ViewModel应该如何实现，如果我们用接口去规定一个VM应该有什么属性，那会导致你的VM不得不包含一个名字已经确定的，但与业务有关的属性。我们在设计中或是支持让用户通过编译绑定路径的方式去自定义，或者是保证我们限定的接口完全与实际业务无关
        另外一方面呢，Ursa的实现并不依赖任何MVVM框架。无论是Prism还是CommunityToolkit或是ReactiveUI，都可以和Ursa配合使用，并不会出现您使用的MVVM框架和Ursa内置的什么东西产生冲突的情况。
        但这样会导致有些MVVM框架内置的一些优秀设计没有办法利用起来，那么就引入了Ursa的第二个特点：

        易于与第三方框架实现桥接。比如我们的Dialog，OverlayDialog其实很好用，和WPF MessageBox一样用静态类就能调用出来。但是就是有人希望不要在VM里面直接去使用一个静态类，比如和Prism集成使用之后，我希望能够用依赖注入的方法调用Dialog。那么针对这些情况，Ursa就会提供相应的基础设施和拓展包，方便使用Prism开发者注册和调用Dialog都能够无感地使用Prism相关的开发范式。同样我们也针对ReactiveUI提供了兼容ReactiveWindow和UrsaWindow的拓展包。

        第三个特点，就是原生支持NativeAOT。我们全部使用编译绑定来开发，没有反射的需求，因此Ursa的控件都自然支持NativeAOT，你甚至不需要针对Semi Avalonia和Ursa进行任何剪裁的配置，就可以实现NativeAOT 编译。

        当我们实现了这些功能的之后，终归还是需要给他一个外观，于是作为Semi Avalonia的维护团队，我们默认提供的是基于Semi Design设计风格的主题。但是，Ursa的Semi主题并不与Semi Avalonia这个库强绑定，实际上如果你有一个基于Semi Avalonia的Design Token实现了自己的主题库，那么即便不引用Semi Avalonia本体，Ursa 的Semi主题依然可以生效。并且Ursa大量复用Semi Avalonia的semantic token，比如Ursa中的多选ComboBox，那么他所使用的动态资源都是原生ComboBox相关的，这样您基于自己公司业务打造的主题在Ursa的Semi主题中也是和谐的。也正式因为这个良好的基础设施，Ursa很快就完成了Semi高对比度配色的适配。

        可惜的是由于我们人力有限，很多人会认为Ursa与Semi是强绑定的。但这是一种错误的认知。Ursa本身作为一个无样式控件库，是可以基于任何设计风格进行设计的。为了打破这种错误的认知，我们也是加班加点打造了一个Ursa的全新样式库。这就是与Classic Avalonia联合开发的Ursa Classic主题，应用这个主题，你可以重现Windows95时代的控件样式。我们只需要利用Classic Avalonia项目中提供的图元，就可以将Ursa打造成Classic风格的控件。

        并且，这并不是一个必须全局使用的主题，他甚至可以在任何局部范围内使用，比如我们这个Demo程序本身是使用Semi主题来进行打造的，但这并不妨碍我们直接对其中一个部分应用Classic主题，这就是我们无样式设计理念所能够达到的效果。

        这个Classic主题我们仍在开发当中，但是很快就能和大家见面了。

        那么以上就是我们过去一年中重点开发的Semi 和 Ursa两个开源项目，希望大家能够对我们的设计体系有进一步的了解，我们的项目已经支持了很多企业项目落地，欢迎您的下一个跨平台桌面应用使用Semi 和Ursa实现生产力的美学进化。
        """;

    public FQA()
    {
        InitializeComponent();
    }
}