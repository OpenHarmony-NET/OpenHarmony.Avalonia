export const setStartDocumentViewPicker: (cbFn: (maxSelectNumber: number, defaultFilePathUri: string,
  fileSuffixFilters: Array<string>, selectMode: number, authMod: boolean) =>  void) => void;
export const setPickerResult: (result: Array<string>) => void;