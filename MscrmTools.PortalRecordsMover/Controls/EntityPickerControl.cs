using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using MscrmTools.PortalRecordsMover.AppCode;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MscrmTools.PortalRecordsMover.Controls
{
    public partial class EntityPickerControl : UserControl
    {
        #region Variables

        private readonly List<ListViewItem> items = new List<ListViewItem>();

        #endregion Variables

        #region Constructor

        public EntityPickerControl()
        {
            InitializeComponent();
        }

        public EntityPickerControl(IOrganizationService service) : this()
        {
            Service = service;
        }

        #endregion Constructor

        #region Properties

        public List<EntityMetadata> Metadata { get; private set; }

        public List<EntityMetadata> SelectedMetadatas
        {
            get { return lvEntities.CheckedItems.Cast<ListViewItem>().Select(i => i.Tag as EntityMetadata).ToList(); }
        }

        public IOrganizationService Service { get; set; }

        #endregion Properties

        #region Events

        private void llClear_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            foreach (ListViewItem item in lvEntities.Items)
            {
                item.Checked = false;
            }
        }

        private void llSelectAll_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            foreach (ListViewItem item in lvEntities.Items)
            {
                item.Checked = true;
            }
        }

        #endregion Events

        #region Methods

        public void FillList()
        {
            lvEntities.Items.Clear();
            lvEntities.Items.AddRange(items.ToArray());
        }

        public void LoadEntities(ExportSettings settings)
        {
            Metadata = MetadataManager.GetEntitiesList(Service);

            items.Clear();

            foreach (var emd in Metadata.Where(m => m.IsIntersect == null || m.IsIntersect.Value == false))
            {
                if (emd.LogicalName == "annotation")
                    continue;

                items.Add(new ListViewItem(emd.DisplayName?.UserLocalizedLabel?.Label ?? emd.SchemaName)
                {
                    Tag = emd,
                    Checked = settings.SelectedEntities.Contains(emd.LogicalName)
                });
            }
        }

        public void SelectItems(List<string> entities)
        {
            foreach (ListViewItem item in lvEntities.Items)
            {
                item.Checked = entities.Contains(((EntityMetadata)item.Tag).LogicalName);
            }
        }

        #endregion Methods
    }
}