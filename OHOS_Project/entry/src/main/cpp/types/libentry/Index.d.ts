import { common } from '@kit.AbilityKit';
export const setStartDocumentViewPicker: (cbFn: (maxSelectNumber: number, defaultFilePathUri: string,
  fileSuffixFilters: Array<string>, selectMode: number, authMod: boolean) => void) => void;

export const setStartDocumentViewPickerSaveMode: (cbFn: (newFileNames: Array<string>, defaultFilePathUri: string,
  fileSuffixChoices: Array<string>, pickerMode: number) => void) => void;

export const setPickerResult: (result: Array<string>) => void;

export const setColor: (colorMode: number) => void;
export const setInputPaneHeight: (keyboardHeightChange: number) => void;
export const setRootArkTSEnv: () => void;
export const setUIContext: (uiContext:common.UIAbilityContext) => void;