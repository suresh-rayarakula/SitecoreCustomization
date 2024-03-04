// Decompiled with JetBrains decompiler
// Type: Sitecore.Shell.Applications.Layouts.DeviceEditor.DeviceEditorForm
// Assembly: Sitecore.Client, Version=18.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 666583AC-FDFF-4158-90AF-08BF89F4B85A

using Microsoft.Extensions.DependencyInjection;
using Sitecore;
using Sitecore.Collections;
using Sitecore.Data;
using Sitecore.Data.Databases;
using Sitecore.Data.Items;
using Sitecore.Data.Templates;
using Sitecore.DependencyInjection;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Layouts;
using Sitecore.Pipelines.RenderDeviceEditorRendering;
using Sitecore.Resources;
using Sitecore.Rules;
using Sitecore.SecurityModel;
using Sitecore.Shell.Applications.Dialogs;
using Sitecore.Shell.Applications.Dialogs.ItemLister;
using Sitecore.Shell.Applications.Dialogs.Personalize;
using Sitecore.Shell.Applications.Layouts.DeviceEditor;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web;
using Sitecore.Web.UI;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Pages;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.XmlControls;
using Sitecore.Xml.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security.AntiXss;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Xml.Linq;
using Control = Sitecore.Web.UI.HtmlControls.Control;
namespace SitecoreCustomization.Models
{
    public class CustomDeviceEditorForm : DialogForm
    {
        private const string CommandName = "device:settestdetails";
        private static BaseDeviceTestEditor _deviceTestEditor;

        public CustomDeviceEditorForm()
          : this(ServiceLocator.ServiceProvider.GetService<BaseDeviceTestEditor>())
        {
        }

        public CustomDeviceEditorForm(BaseDeviceTestEditor deviceTestEditor)
        {
            this.DatabaseHelper = new DatabaseHelper();
            CustomDeviceEditorForm._deviceTestEditor = deviceTestEditor;
        }

        public ArrayList Controls
        {
            get => (ArrayList)Context.ClientPage.ServerProperties[nameof(Controls)];
            set
            {
                Assert.ArgumentNotNull((object)value, nameof(value));
                Context.ClientPage.ServerProperties[nameof(Controls)] = (object)value;
            }
        }

        public string DeviceID
        {
            get => StringUtil.GetString(Context.ClientPage.ServerProperties[nameof(DeviceID)]);
            set
            {
                Assert.ArgumentNotNullOrEmpty(value, nameof(value));
                Context.ClientPage.ServerProperties[nameof(DeviceID)] = (object)value;
            }
        }

        public int SelectedIndex
        {
            get => MainUtil.GetInt(Context.ClientPage.ServerProperties[nameof(SelectedIndex)], -1);
            set => Context.ClientPage.ServerProperties[nameof(SelectedIndex)] = (object)value;
        }

        public string UniqueId
        {
            get => StringUtil.GetString(Context.ClientPage.ServerProperties["PlaceholderUniqueID"]);
            set
            {
                Assert.ArgumentNotNullOrEmpty(value, nameof(value));
                Context.ClientPage.ServerProperties["PlaceholderUniqueID"] = (object)value;
            }
        }

        protected DatabaseHelper DatabaseHelper { get; set; }

        protected TreePicker Layout { get; set; }

        protected Scrollbox Placeholders { get; set; }

        protected Scrollbox Renderings { get; set; }

        protected Button Test { get; set; }

        protected Button Personalize { get; set; }

        protected Button btnEdit { get; set; }

        protected Button btnChange { get; set; }

        protected Button btnRemove { get; set; }

        protected Button MoveUp { get; set; }

        protected Button MoveDown { get; set; }

        protected Button phEdit { get; set; }

        protected Button phRemove { get; set; }

        [HandleMessage("device:add", true)]
        protected void Add(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull((object)args, nameof(args));
            if (args.IsPostBack)
            {
                if (!args.HasResult)
                    return;
                string[] strArray = args.Result.Split(',');
                string str1 = strArray[0];
                string str2 = strArray[1].Replace("-c-", ",");
                bool flag = strArray[2] == "1";
                LayoutDefinition layoutDefinition = CustomDeviceEditorForm.GetLayoutDefinition();
                DeviceDefinition device = layoutDefinition.GetDevice(this.DeviceID);
                RenderingDefinition renderingDefinition = new RenderingDefinition()
                {
                    ItemID = str1,
                    Placeholder = str2
                };
                device.AddRendering(renderingDefinition);
                CustomDeviceEditorForm.SetDefinition(layoutDefinition);
                this.Refresh();
                if (flag)
                {
                    ArrayList renderings = device.Renderings;
                    if (renderings != null)
                    {
                        this.SelectedIndex = renderings.Count - 1;
                        Context.ClientPage.SendMessage((object)this, "device:edit");
                    }
                }
                Registry.SetString("/Current_User/SelectRendering/Selected", str1);
            }
            else
            {
                SelectRenderingOptions renderingOptions = new SelectRenderingOptions()
                {
                    ShowOpenProperties = true,
                    ShowPlaceholderName = true,
                    PlaceholderName = string.Empty
                };
                string str = Registry.GetString("/Current_User/SelectRendering/Selected");
                if (!string.IsNullOrEmpty(str))
                    ((SelectItemOptions)renderingOptions).SelectedItem = Client.ContentDatabase.GetItem(str);
                SheerResponse.ShowModalDialog(((SelectItemOptions)renderingOptions).ToUrlString(Client.ContentDatabase).ToString(), true);
                args.WaitForPostBack();
            }
        }

        [HandleMessage("device:addplaceholder", true)]
        protected void AddPlaceholder(ClientPipelineArgs args)
        {
            if (args.IsPostBack)
            {
                if (string.IsNullOrEmpty(args.Result) || !(args.Result != "undefined"))
                    return;
                LayoutDefinition layoutDefinition = CustomDeviceEditorForm.GetLayoutDefinition();
                DeviceDefinition device = layoutDefinition.GetDevice(this.DeviceID);
                string str;
                Item dialogResult = SelectPlaceholderSettingsOptions.ParseDialogResult(args.Result, Client.ContentDatabase, out str);
                if (dialogResult == null || string.IsNullOrEmpty(str))
                    return;
                PlaceholderDefinition placeholderDefinition = new PlaceholderDefinition()
                {
                    UniqueId = ID.NewID.ToString(),
                    MetaDataItemId = dialogResult.ID.ToString(),
                    Key = str
                };
                device.AddPlaceholder(placeholderDefinition);
                CustomDeviceEditorForm.SetDefinition(layoutDefinition);
                this.Refresh();
            }
            else
            {
                SelectPlaceholderSettingsOptions placeholderSettingsOptions = new SelectPlaceholderSettingsOptions();
                placeholderSettingsOptions.IsPlaceholderKeyEditable = true;
                ((SelectItemOptions)placeholderSettingsOptions).Parameters = CustomDeviceEditorForm.GetParameters();
                SheerResponse.ShowModalDialog(((SelectItemOptions)placeholderSettingsOptions).ToUrlString().ToString(), "460px", "460px", string.Empty, true);
                args.WaitForPostBack();
            }
        }

        [HandleMessage("device:change", true)]
        protected void Change(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull((object)args, nameof(args));
            if (this.SelectedIndex < 0)
                return;
            LayoutDefinition layoutDefinition = CustomDeviceEditorForm.GetLayoutDefinition();
            ArrayList renderings = layoutDefinition.GetDevice(this.DeviceID).Renderings;
            if (renderings == null || !(renderings[this.SelectedIndex] is RenderingDefinition renderingDefinition) || string.IsNullOrEmpty(renderingDefinition.ItemID))
                return;
            if (args.IsPostBack)
            {
                if (!args.HasResult)
                    return;
                string[] strArray = args.Result.Split(',');
                renderingDefinition.ItemID = strArray[0];
                bool flag = strArray[2] == "1";
                CustomDeviceEditorForm.SetDefinition(layoutDefinition);
                this.Refresh();
                if (!flag)
                    return;
                Context.ClientPage.SendMessage((object)this, "device:edit");
            }
            else
            {
                SelectRenderingOptions renderingOptions = new SelectRenderingOptions();
                renderingOptions.ShowOpenProperties = true;
                renderingOptions.ShowPlaceholderName = false;
                renderingOptions.PlaceholderName = string.Empty;
                ((SelectItemOptions)renderingOptions).SelectedItem = Client.ContentDatabase.GetItem(renderingDefinition.ItemID);
                SheerResponse.ShowModalDialog(((SelectItemOptions)renderingOptions).ToUrlString(Client.ContentDatabase).ToString(), true);
                args.WaitForPostBack();
            }
        }

        [HandleMessage("device:edit", true)]
        protected void Edit(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull((object)args, nameof(args));
            if (!new RenderingParameters()
            {
                Args = args,
                DeviceId = this.DeviceID,
                SelectedIndex = this.SelectedIndex,
                Item = UIUtil.GetItemFromQueryString(Client.ContentDatabase)
            }.Show())
                return;
            this.Refresh();
        }

        [HandleMessage("device:editplaceholder", true)]
        protected void EditPlaceholder(ClientPipelineArgs args)
        {
            if (string.IsNullOrEmpty(this.UniqueId))
                return;
            LayoutDefinition layoutDefinition = CustomDeviceEditorForm.GetLayoutDefinition();
            PlaceholderDefinition placeholder = layoutDefinition.GetDevice(this.DeviceID).GetPlaceholder(this.UniqueId);
            if (placeholder == null)
                return;
            if (args.IsPostBack)
            {
                if (string.IsNullOrEmpty(args.Result) || !(args.Result != "undefined"))
                    return;
                string str;
                Item dialogResult = SelectPlaceholderSettingsOptions.ParseDialogResult(args.Result, Client.ContentDatabase, out str);
                if (dialogResult == null)
                    return;
                placeholder.MetaDataItemId = dialogResult.Paths.FullPath;
                placeholder.Key = str;
                CustomDeviceEditorForm.SetDefinition(layoutDefinition);
                this.Refresh();
            }
            else
            {
                Item itemByPathOrId = this.DatabaseHelper.GetItemByPathOrId(Client.ContentDatabase, placeholder.MetaDataItemId);
                SelectPlaceholderSettingsOptions placeholderSettingsOptions = new SelectPlaceholderSettingsOptions();
                placeholderSettingsOptions.TemplateForCreating = (Template)null;
                placeholderSettingsOptions.PlaceholderKey = placeholder.Key;
                placeholderSettingsOptions.CurrentSettingsItem = itemByPathOrId;
                ((SelectItemOptions)placeholderSettingsOptions).SelectedItem = itemByPathOrId;
                placeholderSettingsOptions.IsPlaceholderKeyEditable = true;
                ((SelectItemOptions)placeholderSettingsOptions).Parameters = CustomDeviceEditorForm.GetParameters();
                SheerResponse.ShowModalDialog(((SelectItemOptions)placeholderSettingsOptions).ToUrlString().ToString(), "460px", "460px", string.Empty, true);
                args.WaitForPostBack();
            }
        }

        [HandleMessage("device:test", true)]
        protected void SetTest(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull((object)args, nameof(args));
            Assert.IsNotNull((object)CustomDeviceEditorForm._deviceTestEditor, "deviceTestEditor");
            LayoutDefinition layout = CustomDeviceEditorForm._deviceTestEditor.SetTest(args);
            if (layout == null)
                return;
            CustomDeviceEditorForm.SetDefinition(layout);
            this.Refresh();
        }

        protected virtual void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull((object)e, nameof(e));
            base.OnLoad(e);
            if (Context.ClientPage.IsEvent)
                return;
            this.DeviceID = WebUtil.GetQueryString("de");
            DeviceDefinition device = CustomDeviceEditorForm.GetLayoutDefinition().GetDevice(this.DeviceID);
            if (device.Layout != null)
                ((Control)this.Layout).Value = device.Layout;
            ((Control)this.Personalize).Visible = Policy.IsAllowed("Page Editor/Extended features/Personalization");
            Command command = CommandManager.GetCommand("device:settestdetails", false);
            ((Control)this.Test).Visible = command != null && command.QueryState(CommandContext.Empty) != CommandState.Hidden;
            this.Refresh();
            this.SelectedIndex = -1;
        }

        protected virtual void OnOK(object sender, EventArgs args)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull((object)args, nameof(args));
            if (((Control)this.Layout).Value.Length > 0)
            {
                Item obj = Client.ContentDatabase.GetItem(((Control)this.Layout).Value);
                if (obj == null)
                {
                    Context.ClientPage.ClientResponse.Alert("Layout not found.");
                    return;
                }
                if (ID.Equals(obj.TemplateID, TemplateIDs.Folder) || ID.Equals(obj.TemplateID, TemplateIDs.Node))
                {
                    Context.ClientPage.ClientResponse.Alert(Translate.Text("\"{0}\" is not a layout.", new object[1]
                    {
            (object) obj.GetUIDisplayName()
                    }));
                    return;
                }
            }
            LayoutDefinition layoutDefinition = CustomDeviceEditorForm.GetLayoutDefinition();
            DeviceDefinition device = layoutDefinition.GetDevice(this.DeviceID);
            ArrayList renderings = device.Renderings;
            if (renderings != null && renderings.Count > 0 && ((Control)this.Layout).Value.Length == 0)
            {
                Context.ClientPage.ClientResponse.Alert("You must specify a layout when you specify renderings.");
            }
            else
            {
                device.Layout = ((Control)this.Layout).Value;
                CustomDeviceEditorForm.SetDefinition(layoutDefinition);
                Context.ClientPage.ClientResponse.SetDialogValue("yes");
                base.OnOK(sender, args);
            }
        }

        [ProcessorMethod]
        protected void OnPlaceholderClick(string uniqueId)
        {
            Assert.ArgumentNotNullOrEmpty(uniqueId, nameof(uniqueId));
            if (!string.IsNullOrEmpty(this.UniqueId))
                SheerResponse.SetStyle("ph_" + (object)ID.Parse(this.UniqueId).ToShortID(), "background", string.Empty);
            this.UniqueId = uniqueId;
            if (!string.IsNullOrEmpty(uniqueId))
                SheerResponse.SetStyle("ph_" + (object)ID.Parse(uniqueId).ToShortID(), "background", "#D0EBF6");
            this.UpdatePlaceholdersCommandsState();
        }

        [ProcessorMethod]
        protected void OnRenderingClick(string index, string ItemID)
        {
            Assert.ArgumentNotNull((object)index, nameof(index));
            if (this.SelectedIndex >= 0)
                SheerResponse.SetStyle(StringUtil.GetString(this.Controls[this.SelectedIndex]), "background", string.Empty);
            this.SelectedIndex = MainUtil.GetInt(index, -1);
            if (this.SelectedIndex >= 0)
                SheerResponse.SetStyle(StringUtil.GetString(this.Controls[this.SelectedIndex]), "background", "#D0EBF6");
            this.UpdateRenderingsCommandsState(ItemID);
        }

        [HandleMessage("device:personalize", true)]
        protected void PersonalizeControl(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull((object)args, nameof(args));
            if (this.SelectedIndex < 0)
                return;
            LayoutDefinition layoutDefinition = CustomDeviceEditorForm.GetLayoutDefinition();
            ArrayList renderings = layoutDefinition.GetDevice(this.DeviceID).Renderings;
            if (renderings == null || !(renderings[this.SelectedIndex] is RenderingDefinition renderingDefinition) || string.IsNullOrEmpty(renderingDefinition.ItemID) || string.IsNullOrEmpty(renderingDefinition.UniqueId))
                return;
            if (args.IsPostBack)
            {
                if (!args.HasResult)
                    return;
                XElement ruleSet = XElement.Parse(args.Result);
                renderingDefinition.Rules = CustomDeviceEditorForm.HasConfiguredRules(ruleSet) ? ruleSet : (XElement)null;
                CustomDeviceEditorForm.SetDefinition(layoutDefinition);
                this.Refresh();
            }
            else
            {
                Item itemFromQueryString = UIUtil.GetItemFromQueryString(Client.ContentDatabase);
                string str = itemFromQueryString != null ? itemFromQueryString.Uri.ToString() : string.Empty;
                SheerResponse.ShowModalDialog(new PersonalizeOptions()
                {
                    SessionHandle = CustomDeviceEditorForm.GetSessionHandle(),
                    DeviceId = this.DeviceID,
                    RenderingUniqueId = renderingDefinition.UniqueId,
                    ContextItemUri = str
                }.ToUrlString().ToString(), "980px", "712px", string.Empty, true);
                args.WaitForPostBack();
            }
        }

        [HandleMessage("device:remove")]
        protected void Remove(Message message)
        {
            Assert.ArgumentNotNull(message, nameof(message));
            int selectedIndex = this.SelectedIndex;
            if (selectedIndex < 0)
                return;

            LayoutDefinition layoutDefinition = CustomDeviceEditorForm.GetLayoutDefinition();
            ArrayList renderings = layoutDefinition.GetDevice(this.DeviceID).Renderings;
            if (renderings == null || selectedIndex < 0 || selectedIndex >= renderings.Count)
                return;
            renderings.RemoveAt(selectedIndex);
            if (selectedIndex >= 0)
                --this.SelectedIndex;
            CustomDeviceEditorForm.SetDefinition(layoutDefinition);
            this.Refresh();
        }

        [HandleMessage("device:removeplaceholder")]
        protected void RemovePlaceholder(Message message)
        {
            Assert.ArgumentNotNull((object)message, nameof(message));
            if (string.IsNullOrEmpty(this.UniqueId))
                return;
            LayoutDefinition layoutDefinition = CustomDeviceEditorForm.GetLayoutDefinition();
            DeviceDefinition device = layoutDefinition.GetDevice(this.DeviceID);
            PlaceholderDefinition placeholder = device.GetPlaceholder(this.UniqueId);
            if (placeholder == null)
                return;
            device.Placeholders?.Remove((object)placeholder);
            CustomDeviceEditorForm.SetDefinition(layoutDefinition);
            this.Refresh();
        }

        [HandleMessage("device:sortdown")]
        protected void SortDown(Message message)
        {
            Assert.ArgumentNotNull((object)message, nameof(message));
            if (this.SelectedIndex < 0)
                return;
            LayoutDefinition layoutDefinition = CustomDeviceEditorForm.GetLayoutDefinition();
            ArrayList renderings = layoutDefinition.GetDevice(this.DeviceID).Renderings;
            if (renderings == null || this.SelectedIndex >= renderings.Count - 1 || !(renderings[this.SelectedIndex] is RenderingDefinition renderingDefinition))
                return;
            renderings.Remove((object)renderingDefinition);
            renderings.Insert(this.SelectedIndex + 1, (object)renderingDefinition);
            ++this.SelectedIndex;
            CustomDeviceEditorForm.SetDefinition(layoutDefinition);
            this.Refresh();
        }

        [HandleMessage("device:sortup")]
        protected void SortUp(Message message)
        {
            Assert.ArgumentNotNull((object)message, nameof(message));
            if (this.SelectedIndex <= 0)
                return;
            LayoutDefinition layoutDefinition = CustomDeviceEditorForm.GetLayoutDefinition();
            ArrayList renderings = layoutDefinition.GetDevice(this.DeviceID).Renderings;
            if (renderings == null || !(renderings[this.SelectedIndex] is RenderingDefinition renderingDefinition))
                return;
            renderings.Remove((object)renderingDefinition);
            renderings.Insert(this.SelectedIndex - 1, (object)renderingDefinition);
            --this.SelectedIndex;
            CustomDeviceEditorForm.SetDefinition(layoutDefinition);
            this.Refresh();
        }

        private static LayoutDefinition GetLayoutDefinition()
        {
            string sessionString = WebUtil.GetSessionString(CustomDeviceEditorForm.GetSessionHandle());
            Assert.IsNotNull((object)sessionString, "layout definition");
            return LayoutDefinition.Parse(sessionString);
        }

        private static string GetSessionHandle() => "SC_DEVICEEDITOR";

        private static string GetParameters()
        {
            SafeDictionary<string> queryString = WebUtil.ParseQueryString(HttpContext.Current.Request.Url.Query);
            return ((SafeDictionary<string, string>)queryString).ContainsKey("id") ? "contextItemId" + "=" + HttpUtility.UrlDecode(((SafeDictionary<string, string>)queryString)["id"]) : (string)null;
        }

        private static void SetDefinition(LayoutDefinition layout)
        {
            Assert.ArgumentNotNull((object)layout, nameof(layout));
            string xml = ((XmlSerializable)layout).ToXml();
            WebUtil.SetSessionValue(CustomDeviceEditorForm.GetSessionHandle(), (object)xml);
        }

        private static bool HasRenderingRules(RenderingDefinition definition)
        {
            if (definition.Rules == null)
                return false;
            foreach (XContainer xcontainer in new RulesDefinition(definition.Rules.ToString()).GetRules().Where<XElement>((Func<XElement, bool>)(rule => rule.Attribute((XName)"uid").Value != ItemIDs.Null.ToString())))
            {
                XElement xelement = xcontainer.Descendants((XName)"actions").FirstOrDefault<XElement>();
                if (xelement != null && xelement.Descendants().Any<XElement>())
                    return true;
            }
            return false;
        }

        private static bool HasConfiguredRules(XElement ruleSet)
        {
            List<XElement> list = ruleSet.Elements().ToList<XElement>();
            ID id;
            return list.Count != 1 || !ID.TryParse(list[0].Attribute((XName)"uid")?.Value, out id) || !ID.Equals(id, ID.Null) || list[0].Descendants((XName)"action").Any<XElement>();
        }

        private void Refresh()
        {
            ((Control)this.Renderings).Controls.Clear();
            ((Control)this.Placeholders).Controls.Clear();
            this.Controls = new ArrayList();
            DeviceDefinition device = CustomDeviceEditorForm.GetLayoutDefinition().GetDevice(this.DeviceID);
            if (device.Renderings == null)
            {
                SheerResponse.SetOuterHtml("Renderings", (Control)this.Renderings);
                SheerResponse.SetOuterHtml("Placeholders", (Control)this.Placeholders);
                SheerResponse.Eval("if (!scForm.browser.isIE) { scForm.browser.initializeFixsizeElements(); }");
            }
            else
            {
                int selectedIndex = this.SelectedIndex;
                this.RenderRenderings(device, selectedIndex, 0);
                this.RenderPlaceholders(device);
                //Customization : Getting the rendering item based on the selected rendering index.
                string itemId = selectedIndex > 0 ? ((Sitecore.Layouts.RenderingDefinition)device.Renderings[selectedIndex]).ItemID.ToString() : null;
                this.UpdateRenderingsCommandsState(itemId); // updated with itemID
                this.UpdatePlaceholdersCommandsState();
                SheerResponse.SetOuterHtml("Renderings", (Control)this.Renderings);
                SheerResponse.SetOuterHtml("Placeholders", (Control)this.Placeholders);
                SheerResponse.Eval("if (!scForm.browser.isIE) { scForm.browser.initializeFixsizeElements(); }");
            }
        }

        private void RenderPlaceholders(DeviceDefinition deviceDefinition)
        {
            Assert.ArgumentNotNull((object)deviceDefinition, nameof(deviceDefinition));
            ArrayList placeholders = deviceDefinition.Placeholders;
            if (placeholders == null)
                return;
            foreach (PlaceholderDefinition placeholderDefinition in placeholders)
            {
                Item obj = (Item)null;
                string metaDataItemId = placeholderDefinition.MetaDataItemId;
                if (!string.IsNullOrEmpty(metaDataItemId))
                    obj = this.DatabaseHelper.GetItemByPathOrId(Client.ContentDatabase, metaDataItemId);
                XmlControl webControl = Resource.GetWebControl("DeviceRendering") as XmlControl;
                Assert.IsNotNull((object)webControl, typeof(XmlControl));
                ((Control)this.Placeholders).Controls.Add(webControl);
                ID id = ID.Parse(placeholderDefinition.UniqueId);
                if (placeholderDefinition.UniqueId == this.UniqueId)
                    webControl["Background"] = (object)"#D0EBF6";
                string str = "ph_" + (object)id.ToShortID();
                webControl["ID"] = (object)str;
                webControl["Header"] = (object)AntiXssEncoder.HtmlEncode(placeholderDefinition.Key, true);
                webControl["Click"] = (object)("OnPlaceholderClick(\"" + placeholderDefinition.UniqueId + "\")");
                webControl["DblClick"] = (object)"device:editplaceholder";
                webControl["Icon"] = obj == null ? (object)"Imaging/24x24/layer_blend.png" : (object)((Appearance)obj.Appearance).Icon;
            }
        }

        private void RenderRenderings(DeviceDefinition deviceDefinition, int selectedIndex, int index)
        {
            Assert.ArgumentNotNull((object)deviceDefinition, nameof(deviceDefinition));
            ArrayList renderings = deviceDefinition.Renderings;
            if (renderings == null)
                return;
            foreach (RenderingDefinition renderingDefinition in renderings)
            {
                if (renderingDefinition.ItemID != null)
                {
                    Item obj = Client.ContentDatabase.GetItem(renderingDefinition.ItemID);
                    XmlControl webControl = Resource.GetWebControl("DeviceRendering") as XmlControl;
                    Assert.IsNotNull((object)webControl, typeof(XmlControl));
                    HtmlGenericControl child1 = new HtmlGenericControl("div");
                    child1.Style.Add("padding", "0");
                    child1.Style.Add("margin", "0");
                    child1.Style.Add("border", "0");
                    child1.Style.Add("position", "relative");
                    child1.Controls.Add(webControl);
                    string uniqueId = Control.GetUniqueID("R");
                    ((Control)this.Renderings).Controls.Add(child1);
                    child1.ID = Control.GetUniqueID("C");
                    // Customization : adding item on OnRenderingClick.
                    // webControl["Click"] = (object)("OnRenderingClick(\"" + (object)index + "\")");
                    webControl["Click"] = (object)("OnRenderingClick(\"" + (object)index + "\", \"" + (object)renderingDefinition.ItemID + "\")");

                    webControl["DblClick"] = (object)"device:edit";
                    if (index == selectedIndex)
                        webControl["Background"] = (object)"#D0EBF6";
                    this.Controls.Add((object)uniqueId);
                    if (obj != null)
                    {
                        webControl["ID"] = (object)uniqueId;
                        webControl["Icon"] = (object)((Appearance)obj.Appearance).Icon;
                        webControl["Header"] = (object)obj.GetUIDisplayName();
                        webControl["Placeholder"] = renderingDefinition.Placeholder != null ? (object)WebUtil.SafeEncode(renderingDefinition.Placeholder) : (object)(string)null;
                    }
                    else
                    {
                        webControl["ID"] = (object)uniqueId;
                        webControl["Icon"] = (object)"Applications/24x24/forbidden.png";
                        webControl["Header"] = (object)"Unknown rendering";
                        webControl["Placeholder"] = (object)string.Empty;
                    }
                    if (renderingDefinition.Rules != null && !renderingDefinition.Rules.IsEmpty)
                    {
                        int num = renderingDefinition.Rules.Elements((XName)"rule").Count<XElement>();
                        if (num > 1)
                        {
                            HtmlGenericControl child2 = new HtmlGenericControl("span");
                            if (num > 9)
                                child2.Attributes["class"] = "scConditionContainer scLongConditionContainer";
                            else
                                child2.Attributes["class"] = "scConditionContainer";
                            child2.InnerText = num.ToString();
                            child1.Controls.Add(child2);
                        }
                    }
                    RenderDeviceEditorRenderingPipeline.Run(renderingDefinition, webControl, child1);
                    ++index;
                }
            }
        }
        // Customization : Included the 'ItemID' as an optional parameter.
        private void UpdateRenderingsCommandsState(string ItemID = null)
        {
            if (this.SelectedIndex < 0)
            {
                this.ChangeButtonsState(true);
            }
            else
            {
                ArrayList renderings = CustomDeviceEditorForm.GetLayoutDefinition().GetDevice(this.DeviceID).Renderings;
                if (renderings == null)
                    this.ChangeButtonsState(true);
                else if (!(renderings[this.SelectedIndex] is RenderingDefinition definition))
                {
                    this.ChangeButtonsState(true);
                }
                else
                {
                    this.ChangeButtonsState(false);
                    ((Control)this.Personalize).Disabled = !string.IsNullOrEmpty(definition.MultiVariateTest);
                    ((Control)this.Test).Disabled = CustomDeviceEditorForm.HasRenderingRules(definition);
                    // Customization : Check if the current user belongs to the specific role that allows removal of a specific rendering..
                    // Make a note here: The Item ID is the rendering ID for the specific rendering you want to restrict. 
                    if (ItemID != null && ItemID == "{xxxxxxxx-xxx-xxx-xxx-xxxxxxx}" && IsUserInRole("sitecore\\Author"))
                    {
                        ((Control)this.btnRemove).Disabled = true;
                        ((Control)this.MoveDown).Disabled = true;
                        ((Control)this.MoveUp).Disabled = true;
                        ((Control)this.btnChange).Disabled = true;
                    }
                }
            }
        }
        // Customization : Method to check if the current user belongs to a specific role
        private bool IsUserInRole(string roleName)
        {
            return Context.User.IsInRole(roleName);
        }

        private void UpdatePlaceholdersCommandsState()
        {
            ((Control)this.phEdit).Disabled = string.IsNullOrEmpty(this.UniqueId);
            ((Control)this.phRemove).Disabled = string.IsNullOrEmpty(this.UniqueId);
        }

        private void ChangeButtonsState(bool disable)
        {
            ((Control)this.Personalize).Disabled = disable;
            ((Control)this.btnEdit).Disabled = disable;
            ((Control)this.btnChange).Disabled = disable;
            ((Control)this.btnRemove).Disabled = disable;
            ((Control)this.MoveUp).Disabled = disable;
            ((Control)this.MoveDown).Disabled = disable;
            ((Control)this.Test).Disabled = disable;
        }
    }
}