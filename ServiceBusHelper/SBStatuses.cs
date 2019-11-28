namespace ServiceBusHelper
{
    public enum SBServerStatuses
    {
        Undefined = 0,
        Working = 1,
        Stopped = 2
    }

    // 1. Порядок лучше сохранять: 0, 1, 2, ..., N
    // 2. Лучше первым значением ставить Uknown или Undefined, потому что если будут какие-то ошибки в коде, и твой код
    // будет возвращать эту енамку как результат, то вернется в итоге default(SBClientStatuses), что равно WaitingForFile.
    // И тогда клиенты твоего кода не смогут понять, произошла ошибка или что-то "ожидание файла"
    public enum SBClientStatuses
    {
        Undefined = 0,
        UploadingFile = 1,
        WaitingForFile = 2
    }
}
