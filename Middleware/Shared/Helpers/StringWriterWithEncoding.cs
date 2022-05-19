using System.Text;

namespace Middleware.Shared.Helpers
{
    // https://www.csharp411.com/how-to-force-xmlwriter-or-xmltextwriter-to-use-encoding-other-than-utf-16/
    public class StringWriterWithEncoding : StringWriter 
    { 
        private readonly Encoding m_Encoding; 
        
        public StringWriterWithEncoding(StringBuilder sb, Encoding encoding) : base(sb) 
        { 
            m_Encoding = encoding; 
        } 
        
        public override Encoding Encoding 
        { 
            get
            { 
                return m_Encoding; 
            } 
        } 
    } 
}