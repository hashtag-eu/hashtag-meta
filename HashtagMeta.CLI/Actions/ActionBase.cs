namespace HashtagMeta.CLI.Actions {
    public abstract class ActionBase<T> where T : ActionOptionsBase {
        public abstract int Execute(T options);

        protected InputFileType DetermineInputFileType(InputFileType inputFileType, string[] inputFiles) {
            if (inputFileType == InputFileType.Auto) {
                //determine based on input file(s)
                if (inputFiles.Length == 1) {
                    var input = inputFiles[0];
                    var attr = File.GetAttributes(input);
                    if (attr.HasFlag(FileAttributes.Directory)) {
                        return InputFileType.Folder;
                    } else if (input.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)) {
                        return InputFileType.Zip;
                    }
                    return InputFileType.Files;
                } else if (inputFiles.Length > 0) {
                    return InputFileType.Files;
                } else {
                    return InputFileType.Folder;
                }
            }
            return inputFileType;
        }
    }
}
