namespace Birdie
{
    public static class Shared
    {
        public static BirdieLib.BirdieLib BirdieLib
        {
            get
            {
                if (birdieLib == null)
                {
                    birdieLib = new BirdieLib.BirdieLib(scriptMode: true);
                }

                return birdieLib;
            }
            set
            {
                birdieLib = value;
            }
        }
        private static BirdieLib.BirdieLib birdieLib = null;
    }
}
