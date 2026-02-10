namespace HashtagMeta.CLI.Actions {
    public abstract class ActionBase<T> where T : ActionOptionsBase {
        public abstract int Execute(T options);
    }
}
