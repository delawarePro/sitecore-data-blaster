using Sitecore.IO;

namespace Sitecore.DataBlaster.Util.Logging
{
    /// <summary>
    ///     Adds support for unit testing.
    /// </summary>
    public class SitecoreLogFileAppender : log4net.Appender.SitecoreLogFileAppender
    {
        private string m_originalFileName;

        public override string File
        {
            get { return base.File; }
            set
            {
                if (m_originalFileName == null)
                {
                    var str = value;
                    var variables = log4net.Appender.ConfigReader.GetVariables();
                    foreach (var index in variables.Keys)
                    {
                        var oldValue = "$(" + index + ")";
                        str = str.Replace(oldValue, variables[index]);
                    }

                    // Use mapping with unit test support.
                    m_originalFileName = FileUtil.MapPath(str.Trim());
                }
                base.File = m_originalFileName;
            }
        }

        protected override void CloseWriter()
        {
            if (m_qtw == null) return;
            try
            {
                m_qtw.Close();
            }
            catch
            {
                // Ignore multi disposing issues.
            }
        }
    }
}
