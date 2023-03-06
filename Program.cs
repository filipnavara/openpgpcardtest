using PCSC;
using PCSC.Iso7816;
using System.Text;

namespace OpenPGPCard
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var contextFactory = ContextFactory.Instance;
            using (var context = contextFactory.Establish(SCardScope.System))
            {
                Console.WriteLine("Currently connected readers: ");
                var readerNames = context.GetReaders();
                foreach (var readerName in readerNames)
                {
                    Console.WriteLine("\t" + readerName);

                    using (var isoReader = new IsoReader(context, readerName, SCardShareMode.Shared, SCardProtocol.Any, false))
                    {
                        // Select
                        var apduSelect = new CommandApdu(IsoCase.Case3Short, isoReader.ActiveProtocol)
                        {
                            CLA = 0x00, // Class
                            Instruction = InstructionCode.SelectFile,
                            P1 = 0x04, // Parameter 1
                            P2 = 0x00, // Parameter 2
                            Data = new byte[] { 0xD2, 0x76, 0x00, 0x01, 0x24, 0x01 },
                        };

                        var responseSelect = isoReader.Transmit(apduSelect);


                        // Verify PIN1
                        var apduVerify = new CommandApdu(IsoCase.Case3Short, isoReader.ActiveProtocol)
                        {
                            CLA = 0x00, // Class
                            Instruction = InstructionCode.Verify,
                            P1 = 0x00, // Parameter 1
                            P2 = 0x81, // Parameter 2
                            Data = "123456"u8.ToArray(),
                        };

                        string cmd = Convert.ToHexString(apduVerify.ToArray());

                        var responseVerify = isoReader.Transmit(apduVerify);


                        var apdu = new CommandApdu(IsoCase.Case3Short, isoReader.ActiveProtocol)
                        {
                            CLA = 0x00, // Class
                            Instruction = (InstructionCode)0x47,
                            P1 = 0x81, // Parameter 1
                            P2 = 0x00, // Parameter 2
                            Data = new byte[] { 0xB6 /* Signature key */, 0 }
                        };

                        var response = isoReader.Transmit(apdu);
                        Console.WriteLine("SW1 SW2 = {0:X2} {1:X2}", response.SW1, response.SW2);

                        if (response.SW1 == 0x90 && response.SW2 == 0)
                        {
                            var data = response.GetData();
                            Console.WriteLine(Convert.ToHexString(data));
                        }
                        // ..
                    }
                }
            }
        }
    }
}