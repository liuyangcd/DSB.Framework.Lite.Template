namespace SolutionName.Domain
{
    /// <summary>
    /// 数据种子贡献者接口，应用启动时自动执行
    /// </summary>
    public interface IDataSeedContributor
    {
        /// <summary>
        /// 执行种子数据初始化
        /// </summary>
        /// <returns></returns>
        Task SeedAsync();
    }
}
