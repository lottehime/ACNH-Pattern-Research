public class ACNLConvertExample
{
    public void ConvertACPattern()
    {
        byte[] ACNHpattern = File.ReadAllBytes(ACNH_Pattern_Example.nhd); // Read your pattern
        
        byte[] ACNLpattern = new byte[0x26C]; // Create new bytes for ACNL
        
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
        
        Array.Copy(patternDat, 0x54, ACNLbin, 0x2A, 2); // Player ID Hash
        Array.Copy(patternDat, 0x38, ACNLbin, 0x40, 2); // Town ID Hash
        
        byte[] ACNHpalette = new byte[45]; // Create bytes to store ACNH palette
        Array.Copy(ACNHpattern, 0x78, ACNHpalette, 0, 45); // Store them
        
        byte[] ACNLpalette = new byte[15]; // Create bytes to store ACNL palette
        for (int i = 0; i < ACNLpalette.Length; i++)
        {
            ACNLpalette[i] = GetNearestColor(ACNHpalette[i*3], ACNHpalette[i*3 + 1], ACNHpalette[i*3 + 2]);
        }
        Array.Copy(ACNLpalette, 0x00, ACNLpattern, 0x58, 15);  // Store them
        
        Array.Copy(ACNHpattern, 0xA5, ACNLpattern, 0x6C, 512); // Pixel Data
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

}