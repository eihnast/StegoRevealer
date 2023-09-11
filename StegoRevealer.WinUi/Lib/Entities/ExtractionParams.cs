namespace StegoRevealer.WinUi.Lib.Entities
{
    public class ExtractionParams
    {
        private bool _lsb = true;
        private bool _kz = false;

        public bool LsbExtration
        {
            get => _lsb;
            set
            {
                _lsb = value;
                _kz = !_lsb;
            }
        }

        public bool KzExtraction
        {
            get => _kz;
            set
            {
                _kz = value;
                _lsb = !_kz;
            }
        }

        private bool _linear = true;
        private bool _random = false;

        public bool LinearHided
        {
            get => _linear;
            set
            {
                _linear = value;
                _random = !_linear;
            }
        }

        public bool RandomHided
        {
            get => _random;
            set
            {
                _random = value;
                _linear = !_random;
            }
        }

        public int? LsbSeed { get; set; }
        public int? LsbStartIndex { get; set; }
        public int? LsbByteLength { get; set; }

        public int? KzSeed { get; set; }
        public int? KzIndexFirst { get; set; }
        public int? KzIndexSecond { get; set; }
        public double? KzThreshold { get; set; }
    }
}
