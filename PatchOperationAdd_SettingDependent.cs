using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace CultivatorOfTheRim
{
    public class PatchOperationAdd_SettingDependent : PatchOperationPathed
    {
        private enum Order
        {
            Append,
            Prepend
        }

        private XmlContainer value;

        private Order order;

        private string key;

        protected override bool ApplyWorker(XmlDocument xml)
        {
            XmlNode node = value.node;
            try
            {
                if (!CultivatorOfTheRimMod.settings.settingKey.NullOrEmpty() && key != null)
                {
                    if (CultivatorOfTheRimMod.settings.settingKey.Contains(key))
                    {
                        foreach (var item in xml.SelectNodes(xpath))
                        {
                            XmlNode xmlNode = item as XmlNode;
                            if (order == Order.Append)
                            {
                                foreach (XmlNode childNode in node.ChildNodes)
                                {
                                    xmlNode.AppendChild(xmlNode.OwnerDocument.ImportNode(childNode, deep: true));
                                }
                            }
                            else if (order == Order.Prepend)
                            {
                                for (int num = node.ChildNodes.Count - 1; num >= 0; num--)
                                {
                                    xmlNode.PrependChild(xmlNode.OwnerDocument.ImportNode(node.ChildNodes[num], deep: true));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[CTR] Error in PatchOperationAdd_SettingDependent {xpath} failed to apply patch with Key {key}. {ex}");
            }
            return true;
        }
    }
}
