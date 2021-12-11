namespace Blox.ConfigurationNS
{
    public interface IBlockType : IEntityType
    {
        public bool isSolid { get; }
        public bool isFluid { get; }
        public bool isSoil { get; }
        public bool isEmpty { get; }

        public TextureType GetTextureType(BlockFace face);
    }
}