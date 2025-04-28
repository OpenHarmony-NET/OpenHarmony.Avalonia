import { AbilityConstant, ConfigurationConstant, UIAbility, Want } from '@kit.AbilityKit';
import { hilog } from '@kit.PerformanceAnalysisKit';
import { window } from '@kit.ArkUI';
import { picker } from '@kit.CoreFileKit';
import { fileIo as fs } from '@kit.CoreFileKit';
import { common } from '@kit.AbilityKit';
import { BusinessError } from '@kit.BasicServicesKit';
import * as avalonia from 'libentry.so'
import { uri } from '@kit.ArkTS';

const DOMAIN = 0x0000;

function returnNumber() {
  return 10;
}

export default class EntryAbility extends UIAbility {
  onCreate(want: Want, launchParam: AbilityConstant.LaunchParam): void {
    this.context.getApplicationContext().setColorMode(ConfigurationConstant.ColorMode.COLOR_MODE_NOT_SET);
    hilog.info(DOMAIN, 'testTag', '%{public}s', 'Ability onCreate');
    //将文件选择器的触发器注册到.Net中
    avalonia.setStartDocumentViewPicker((maxSelectNumber: number, defaultFilePathUri: string,
      fileSuffixFilters: Array<string>, selectMode: number, authMod: boolean) => {
      const documentSelectOptions = new picker.DocumentSelectOptions();
      // 选择文档的最大数目（可选）。
      documentSelectOptions.maxSelectNumber = maxSelectNumber;
      hilog.info(DOMAIN, 'testTag', 'maxSelectNumber:' + maxSelectNumber);
      // 指定选择的文件或者目录路径（可选）。
      documentSelectOptions.defaultFilePathUri = defaultFilePathUri;
      // 选择文件的后缀类型['后缀类型描述|后缀类型']（可选，不传该参数，默认不过滤，即显示所有文件），若选择项存在多个后缀名，则每一个后缀名之间用英文逗号进行分隔（可选），后缀类型名不能超过100。此外2in1设备支持通过通配符方式['所有文件(*.*)|.*']，表示为显示所有文件，手机暂不支持该配置。
      documentSelectOptions.fileSuffixFilters = fileSuffixFilters
      for (let fileSuffixFiltersElement of fileSuffixFilters) {
        hilog.info(DOMAIN, 'testTag', 'fileSuffixFilters:' + fileSuffixFiltersElement);
      }
      //选择是否对指定文件或目录授权，true为授权，当为true时，defaultFilePathUri为必选参数，拉起文管授权界面；false为非授权（默认为false），拉起常规文管界面（可选），仅支持2in1设备。
      documentSelectOptions.authMode = authMod;
      documentSelectOptions.selectMode = selectMode;
      //批量授权模式，默认为false（非批量授权模式）。当multAuthMode为true时为批量授权模式。当multAuthMode为true时，只有multiUriArray参数生效，其他参数不生效。仅支持手机设备。
      //documentSelectOptions.multiAuthMode = false;
      //需要传入批量授权的uri数组（仅支持文件，文件夹不生效）。配合multAuthMode使用。当multAuthMode为false时，配置该参数不生效。仅支持手机设备。
      //documentSelectOptions.multiUriArray =
      //  ["file://docs/storage/Users/currentUser/test", "file://docs/storage/Users/currentUser/2test"];
      //开启聚合视图模式，支持拉起文件管理应用的聚合视图。默认为DEFAULT，表示该参数不生效，非聚合视图。当该参数置为非DEFAULT时，其他参数不生效。仅支持手机设备。
      //documentSelectOptions.mergeMode = picker.MergeTypeMode.DEFAULT;
      let uris: Array<string> = [];
      let context = getContext(this) as common.Context; // 请确保 getContext(this) 返回结果为 UIAbilityContext
      // 创建文件选择器实例
      const documentViewPicker = new picker.DocumentViewPicker(context);
      documentViewPicker.select(documentSelectOptions).then((documentSelectResult: Array<string>) => {
        //文件选择成功后，返回被选中文档的URI结果集。
        uris = documentSelectResult;
        console.info('documentViewPicker.select to file succeed and uris are:' + uris);
      }).catch((err: BusinessError) => {
        console.error(`Invoke documentViewPicker.select failed, code is ${err.code}, message is ${err.message}`);
      }).finally(() => {
        avalonia.setPickerResult(uris);
      })
    })
    avalonia.setStartDocumentViewPickerSaveMode((newFileNames: Array<string>, defaultFilePathUri: string,
      fileSuffixChoices: Array<string>, pickerMode: number) => {
      let result: Array<string> = [];
      let documentSaveOptions = new picker.DocumentSaveOptions();
      documentSaveOptions.newFileNames = ['DocumentViewPicker01.txt'];
      let documentPicker = new picker.DocumentViewPicker(getContext(this) as common.Context);
      documentPicker.save(documentSaveOptions).then((documentSaveResult: Array<string>) => {
        console.info('DocumentViewPicker.save successfully, documentSaveResult uri: ' +
        JSON.stringify(documentSaveResult));
        result = documentSaveResult;
      }).catch((err: BusinessError) => {
        console.error('DocumentViewPicker.save failed with err: ' + JSON.stringify(err));
      }).finally(() => {
        avalonia.setPickerResult(result);
      });
    });
  }

  onDestroy(): void {
    hilog.info(DOMAIN, 'testTag', '%{public}s', 'Ability onDestroy');
  }

  onWindowStageCreate(windowStage: window.WindowStage): void {
    // Main window is created, set main page for this ability
    hilog.info(DOMAIN, 'testTag', '%{public}s', 'Ability onWindowStageCreate');

    windowStage.loadContent('pages/Index', (err) => {
      if (err.code) {
        hilog.error(DOMAIN, 'testTag', 'Failed to load the content. Cause: %{public}s', JSON.stringify(err));
        return;
      }
      windowStage.getMainWindow().then(w => {
        w.setWindowLayoutFullScreen(true)
        w.setPreferredOrientation(window.Orientation.AUTO_ROTATION)
      })
      hilog.info(DOMAIN, 'testTag', 'Succeeded in loading the content.');
    });
  }

  onWindowStageDestroy(): void {
    // Main window is destroyed, release UI related resources
    hilog.info(DOMAIN, 'testTag', '%{public}s', 'Ability onWindowStageDestroy');
  }

  onForeground(): void {
    // Ability has brought to foreground
    hilog.info(DOMAIN, 'testTag', '%{public}s', 'Ability onForeground');
  }

  onBackground(): void {
    // Ability has back to background
    hilog.info(DOMAIN, 'testTag', '%{public}s', 'Ability onBackground');
  }
}