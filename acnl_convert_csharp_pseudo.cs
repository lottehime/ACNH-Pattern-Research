public class ACNLConvertExample
{
    public byte[] ConvertACPattern()
    {
        byte[] ACNHpattern = File.ReadAllBytes(ACNH_Pattern_Example.nhd); // Read your pattern
        
        byte[] ACNLpattern = null;
        int pixelDataLength = 0;
        switch(ACNHpattern.Length) // Check Pattern Type
        {
            case 0x2A8:
              ACNLpattern = new byte[0x26C]; // Create new bytes for ACNL Normal
              pixelDataLength = 512;
              break;
            case 0x8A8:
              ACNLpattern = new byte[0x870]; // Create new bytes for ACNL Pro
              pixelDataLength = 2048;
              break;  
            default:
              // Malformed file
              return;
        }
        
        switch(ACNHpattern[0x77]) // Check Pattern Type
        {
            case 0x00:
              ACNLpattern[0x69] = 0x09;
              break;
            case 0x12:
              ACNLpattern[0x69] = 0x01;
              break;
            case 0x13:
              ACNLpattern[0x69] = 0x00;
              break;
            case 0x14:
              ACNLpattern[0x69] = 0x02;
              break;
            case 0x15:
              ACNLpattern[0x69] = 0x04;
              break;
            case 0x16:
              ACNLpattern[0x69] = 0x03;
              break;
            case 0x17:
              ACNLpattern[0x69] = 0x05;
              break;
            case 0x18:
              ACNLpattern[0x69] = 0x07;
              break;
            case 0x19:
              ACNLpattern[0x69] = 0x06;
              break;    
            case 0x1E:
              ACNLpattern[0x69] = 0x08;
              break;  
            default:
              // Not a default interoperable type
              return;
              break;
        }
        
        Array.Copy(ACNHpattern, 0x10, ACNLpattern, 0x00, 40); // Pattern Name
        Array.Copy(ACNHpattern, 0x58, ACNLpattern, 0x2C, 18); // Player Name (Trimmed)
        Array.Copy(ACNHpattern, 0x3C, ACNLpattern, 0x42, 18); // Island Name (Trimmed)
        
        Array.Copy(ACNHpattern, 0x54, ACNLpattern, 0x2A, 2); // Player ID Hash (or reset to 0xFF, 0xFF)
        Array.Copy(ACNHpattern, 0x38, ACNLpattern, 0x40, 2); // Town ID Hash
        
        byte[] ACNHpalette = new byte[45]; // Create bytes to store ACNH palette
        Array.Copy(ACNHpattern, 0x78, ACNHpalette, 0, 45); // Store them
        
        byte[] ACNLpalette = new byte[15]; // Create bytes to store ACNL palette
        for (int i = 0; i < ACNLpalette.Length; i++)
        {
            ACNLpalette[i] = GetNearestColor(ACNHpalette[i*3], ACNHpalette[i*3 + 1], ACNHpalette[i*3 + 2]);
        }
        Array.Copy(ACNLpalette, 0x00, ACNLpattern, 0x58, 15);  // Store them
        
        Array.Copy(ACNHpattern, 0xA5, ACNLpattern, 0x6C, pixelDataLength); // Pixel Data

        if(ACNLpattern.Length == 0x870)
            ACNLpattern = DataSortACNL(ACNLpattern);

        return ACNLpattern;
    }
    
    public byte GetNearestColor(byte R, byte G, byte B)
    {
        var convVal = -1;
        var convIndex = -1;
    
        for (int i = 0; i < 256; i++) // Run against the color index
        {
            if (ACNLColors[i] != Color.Empty) // Check we don't pick one of the unused colors
            {
                // Get differences in color value
                var diff = Math.Abs(ACNLColors[i].R - R) + Math.Abs(ACNLColors[i].G - G) + Math.Abs(ACNLColors[i].B - B);
    
                if (convVal == -1 || convVal > diff) // If close enough, pick it
                {
                    convVal = diff;
                    convIndex = i;
                }
            }
        }
    
        return (byte)convIndex;
    }
    
    public Color[] ACNLColors = new Color[256]
    {
        Color.FromArgb ( 0xFF, 0xEE, 0xFF ), // 0x00, refer color table in info above
        // ...and on and on...
        Color.Empty, // 0xFF, refer color table in info above
    };

    public static byte[] DataSortACNL(byte[] data)
    {
        var dataSorted = new byte[0x870];
        Array.Copy(data, 0x00, dataSorted, 0x00, 0x870);

        Dictionary<(int, int), int> OffsetsProTexChunk = new Dictionary<(int, int), int>()
        {
            // Front Chunk
            { ( 0x000, 0x200), 0x200 }, 
            // Back Chunk
            { ( 0x200, 0x400), 0x000 },
            // Front Bottom Chunk
            { ( 0x600, 0x700), 0x600 },
            // Back Bottom Chunk
            { ( 0x700, 0x800), 0x400 },
            // Left Sleeve Chunk
            { ( 0x400, 0x500), 0x500 },
            // Right Sleeve Chunk
            { ( 0x500, 0x600), 0x700 },
        };

        Dictionary<(int, int), int> OffsetsProStandeeTexChunk = new Dictionary<(int, int), int>()
        {
            // Top Left Chunk
            { ( 0x000, 0x200), 0x000 }, 
            // Bottom Left Chunk
            { ( 0x200, 0x400), 0x400 },
            // Top Right Chunk
            { ( 0x400, 0x600), 0x200 },
            // Bottom Right Chunk
            { ( 0x600, 0x800), 0x600 }
        };

        // Check data type
        if (data[0x69] != 0x06 && data[0x69] != 0x07)
        {
            // Iterate pixels
            for (var y = 0; y < 64; y++)
            {
                for (var x = 0; x < 64 / 2; x++)
                {
                    // Ain't a hat? Off we hack!
                    if (data[0x69] != 0x06 && data[0x69] != 0x07)
                    {
                        // Grab offsets for chunks
                        var offset = (x >= 64 / 4 ? 0x200 : 0x0) + (y >= 64 / 2 ? 0x400 : 0x0);
                        int index = offset + x % (64 / 4) + (y % (64 / 2)) * (64 / 4);
                        if (data[0x69] == 0x08) // If it's a standee
                        {
                            var newIndex = index;
                            foreach (var kvp in OffsetsProStandeeTexChunk)
                            {
                                // Match up chunks
                                if (index >= kvp.Value && index < kvp.Value + (kvp.Key.Item2 - kvp.Key.Item1))
                                {
                                    newIndex = (index - kvp.Value) + kvp.Key.Item1;
                                }
                            }
                            dataSorted[0x6C + newIndex] = data[0x6C + index]; // Place chunks
                        }
                        else
                        {
                            var newIndex = index;
                            foreach (var kvp in OffsetsProTexChunk)
                            {
                                // Match up chunks
                                if (index >= kvp.Value && index < kvp.Value + (kvp.Key.Item2 - kvp.Key.Item1))
                                {
                                    newIndex = (index - kvp.Value) + kvp.Key.Item1;
                                }
                            }
                            dataSorted[0x6C + newIndex] = data[0x6C + index]; // Place chunks
                        }
                    }
                }
            }
        }
        else
        {
            // It's a hat? Cut it back!
            Array.Copy(data, 0x6C, dataSorted, 0x6C, 0x200);
        }

        return dataSorted;
    }

}