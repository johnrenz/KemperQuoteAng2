using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace AutoQuoteLibrary.BL
{
    public class QuestionPlugin
    {
        /// <summary>
        /// Provides for Abstract XML that allows for State Specific Overrides
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public XElement Load(String state)
        {
            XDocument questions = XDocument.Load(ConfigurationManager.AppSettings["PluginsPath"] + "\\QuoteFlowData\\Questions\\BaseQuestions.xml");

            if (File.Exists(ConfigurationManager.AppSettings["PluginsPath"] + "\\QuoteFlowData\\Questions\\" + state.ToUpper() + "Questions.xml"))
            {
                XDocument overrides = XDocument.Load(ConfigurationManager.AppSettings["PluginsPath"] + "\\QuoteFlowData\\Questions\\" + state.ToUpper() + "Questions.xml");

                foreach (XElement question in overrides.Root.Elements())
                {
                    String id = question.Attribute("ID").Value;
                    XElement baseQuestion = questions.Root.Elements().Where(node => node.Attribute("ID").Value == id).FirstOrDefault();

                    if (baseQuestion != null)
                    {
                        baseQuestion.ReplaceWith(question);
                    }
                }
            }

            return questions.Root;
        }
    }
}
