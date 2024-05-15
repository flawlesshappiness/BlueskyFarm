public static class StringExtensions
{
    public static int GetLengthWithoutBBCode(this string str)
    {
        int visibleCharacters = 0;

        for (int i = 0; i < str.Length; i++)
        {
            if (str[i] == '[')
            {
                while (i < str.Length && str[i] != ']')
                {
                    i++;
                }
            }

            visibleCharacters++;
        }

        return visibleCharacters;
    }
}
