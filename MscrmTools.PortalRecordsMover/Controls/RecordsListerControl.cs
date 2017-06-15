﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;

namespace MscrmTools.PortalRecordsMover.Controls
{
    public partial class RecordsListerControl : UserControl
    {
        public RecordsListerControl(List<Entity> records, EntityMetadata emd, string layoutXml)
        {
            InitializeComponent();

            var doc = new XmlDocument();
            doc.LoadXml(layoutXml);

            foreach (XmlNode node in doc.SelectNodes("grid/row/cell"))
            {
                lvRecords.Columns.Add(new ColumnHeader
                {
                    Text = emd.Attributes
                        .FirstOrDefault(a => a.LogicalName == node.Attributes["name"].Value)?
                        .DisplayName?.UserLocalizedLabel?.Label ?? node.Attributes["name"].Value,
                    Width = int.Parse(node.Attributes["width"].Value)
                });
            }

            foreach (var record in records)
            {
                var item = new ListViewItem
                {
                    Tag = record,
                    Checked = true
                };

                foreach (XmlNode node in doc.SelectNodes("grid/row/cell"))
                {
                    if (node == node.ParentNode.FirstChild)
                    {
                        item.Text = GetRecordValue(record, node.Attributes["name"].Value, emd);
                    }
                    else
                    {
                        item.SubItems.Add(GetRecordValue(record, node.Attributes["name"].Value, emd));
                    }
                }

                lvRecords.Items.Add(item);
            }
        }

        public RecordsListerControl(List<Entity> records, List<Entity> allRecords, ManyToManyRelationshipMetadata mm, List<EntityMetadata> emds)
        {
            InitializeComponent();

            foreach (var record in records)
            {
                var e1 = mm.Entity1LogicalName;
                var id1 = record.GetAttributeValue<Guid>(mm.Entity1IntersectAttribute);
                var emd1 = emds.First(e => e.LogicalName == e1);
                var name1 = allRecords.FirstOrDefault(r => r.LogicalName == e1 && r.Id == id1)?.GetAttributeValue<string>(emd1.PrimaryNameAttribute) ?? id1.ToString("B");

                var e2 = mm.Entity2LogicalName;
                var id2 = record.GetAttributeValue<Guid>(mm.Entity2IntersectAttribute);
                var emd2 = emds.First(e => e.LogicalName == e2);
                var name2 = allRecords.FirstOrDefault(r => r.LogicalName == e2 && r.Id == id2)?.GetAttributeValue<string>(emd2.PrimaryNameAttribute) ?? id2.ToString("B"); ;

                if (lvRecords.Columns.Count == 0)
                {
                    lvRecords.Columns.Add(new ColumnHeader { Text = emd1.DisplayName?.UserLocalizedLabel?.Label ?? emd1.SchemaName });

                    lvRecords.Columns.Add(new ColumnHeader
                    {
                        Text = emd2.DisplayName?.UserLocalizedLabel?.Label ?? emd2.SchemaName
                    });
                }

                var item = new ListViewItem(name1)
                {
                    Tag = record,
                    SubItems = { name2 },
                    Checked = true
                };

                lvRecords.Items.Add(item);
            }
        }

        public List<Entity> Records
        {
            get { return lvRecords.CheckedItems.Cast<ListViewItem>().Select(i => i.Tag as Entity).ToList(); }
        }

        private void llClear_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            foreach (ListViewItem item in lvRecords.Items)
            {
                item.Checked = false;
            }
        }

        private void llSelectAll_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            foreach (ListViewItem item in lvRecords.Items)
            {
                item.Checked = true;
            }
        }

        private string GetRecordValue(Entity record, string value, EntityMetadata emd)
        {
            var amd = emd.Attributes.FirstOrDefault(a => a.LogicalName == value);
            if (amd == null)
            {
                return record.Contains(value) ? record["value"].ToString() : string.Empty;
            }

            switch (amd.AttributeType.Value)
            {
                case AttributeTypeCode.Boolean:
                    return record.GetAttributeValue<bool>(value).ToString();

                case AttributeTypeCode.Customer:
                case AttributeTypeCode.Lookup:
                case AttributeTypeCode.Owner:
                    return record.GetAttributeValue<EntityReference>(value)?.Name ?? string.Empty;

                case AttributeTypeCode.DateTime:
                    return record.GetAttributeValue<DateTime>(value).ToString();

                case AttributeTypeCode.Decimal:
                    return record.GetAttributeValue<decimal>(value).ToString();

                case AttributeTypeCode.Double:
                    return record.GetAttributeValue<double>(value).ToString();

                case AttributeTypeCode.Integer:
                    return record.GetAttributeValue<int>(value).ToString();

                case AttributeTypeCode.EntityName:
                case AttributeTypeCode.Memo:
                case AttributeTypeCode.String:
                    return record.GetAttributeValue<string>(value);

                case AttributeTypeCode.Picklist:
                    {
                        var ov = record.GetAttributeValue<OptionSetValue>(value);
                        if (ov == null) return string.Empty;
                        var pamd = (PicklistAttributeMetadata)amd;
                        return pamd.OptionSet.Options.First(
                            o => o.Value.Value == ov.Value)
                            .Label?.UserLocalizedLabel?.Label;
                    }
                case AttributeTypeCode.State:
                    {
                        var ov = record.GetAttributeValue<OptionSetValue>(value);
                        if (ov == null) return string.Empty;
                        var pamd = (StateAttributeMetadata)amd;
                        return pamd.OptionSet.Options.First(
                            o => o.Value.Value == ov.Value)
                            .Label.UserLocalizedLabel.Label;
                    }
                case AttributeTypeCode.Status:
                    {
                        var ov = record.GetAttributeValue<OptionSetValue>(value);
                        if (ov == null) return string.Empty;
                        var pamd = (StatusAttributeMetadata)amd;
                        return pamd.OptionSet.Options.First(
                            o => o.Value.Value == ov.Value)
                            .Label.UserLocalizedLabel.Label;
                    }
                default:
                    return record.Contains(value) ? record["value"].ToString() : string.Empty;
            }
        }
    }
}