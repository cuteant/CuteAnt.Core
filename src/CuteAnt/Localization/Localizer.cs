using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using System.Text;
using System.Xml;
using CuteAnt.Localization.Internal;

namespace CuteAnt.Localization
{
	#region -- HmLocalizer<T> abstract class --

	public abstract class HmLocalizer<T> where T : struct
	{
		#region - Fields -

		private static ActiveLocalizerProvider<T> m_localizerProvider;
		private Dictionary<T, String> m_stringTable;

		#endregion

		protected HmLocalizer()
		{
			CreateStringTable();
		}

		#region - Properties -

		public virtual String Language
		{
			get { return "English"; }
		}

		internal Dictionary<T, String> StringTable
		{
			get { return m_stringTable; }
		}

		public static HmLocalizer<T> Active
		{
			get { return m_localizerProvider.GetActiveLocalizer(); }
			set
			{
				if (value == null)
				{
					value = m_localizerProvider.GetActiveLocalizer().CreateResXLocalizer();
				}
				if (Object.ReferenceEquals(m_localizerProvider.GetActiveLocalizer(), value)) { return; }
				m_localizerProvider.SetActiveLocalizer(value);
				RaiseActiveChanged();
			}
		}

		#endregion

		#region - Events -

		public static event EventHandler ActiveChanged;

		public static void RaiseActiveChanged()
		{
			if (ActiveChanged != null)
			{
				ActiveChanged(Active, EventArgs.Empty);
			}
		}

		#endregion

		public static void SetActiveLocalizerProvider(ActiveLocalizerProvider<T> value)
		{
			m_localizerProvider = value;
		}

		public static ActiveLocalizerProvider<T> GetActiveLocalizerProvider()
		{
			return m_localizerProvider;
		}

		protected virtual IEqualityComparer<T> CreateComparer()
		{
			return EqualityComparer<T>.Default;
		}

		protected internal virtual void CreateStringTable()
		{
			m_stringTable = new Dictionary<T, String>(CreateComparer());
			PopulateStringTable();
		}

		protected internal virtual void AddString(T id, String str)
		{
			//m_stringTable.Add(id, str);
			m_stringTable[id] = str;
		}

		public virtual String GetLocalizedString(T id)
		{
			String result;
			if (m_stringTable.TryGetValue(id, out result))
			{
				return result;
			}
			else
			{
				return String.Empty;
			}
		}

		protected internal virtual String GetEnumTypeName()
		{
			return typeof(T).Name;
		}

		public virtual void WriteToXml(String fileName)
		{
			XmlDocument doc = CreateXmlDocument();
			XmlTextWriter writer = new XmlTextWriter(fileName, Encoding.Default);

			try
			{
				writer.Formatting = Formatting.Indented;
				doc.WriteTo(writer);
			}
			finally
			{
				writer.Flush();
				writer.Close();
			}
		}

		public virtual XmlDocument CreateXmlDocument()
		{
			String typeName = GetEnumTypeName();
			XmlDocument doc = new XmlDocument();
			XmlDeclaration decl = doc.CreateXmlDeclaration("1.0", "utf-8", String.Empty);

			doc.AppendChild(decl);
			XmlElement root = doc.CreateElement("root");

			doc.AppendChild(root);
			T[] values = (T[])Enum.GetValues(typeof(T));
			String[] names = Enum.GetNames(typeof(T));
			Int32 count = values.Length;

			for (Int32 i = 0; i < count; i++)
			{
				XmlElement dataEl = doc.CreateElement("data");
				root.AppendChild(dataEl);
				XmlAttribute nameAttr = doc.CreateAttribute("name");
				nameAttr.Value = String.Format("{0}.{1}", typeName, names[i]);
				dataEl.Attributes.Append(nameAttr);
				XmlElement valueEl = doc.CreateElement("value");
				dataEl.AppendChild(valueEl);
				XmlText valueText = doc.CreateTextNode("value");
				valueText.Value = GetLocalizedString(values[i]);
				valueEl.AppendChild(valueText);
			}
			return doc;
		}

		protected internal abstract void PopulateStringTable();

		public abstract HmLocalizer<T> CreateResXLocalizer();
	}

	#endregion

	#region -- HmResXLocalizer<T> abstract class --

	public abstract class HmResXLocalizer<T> : HmLocalizer<T> where T : struct
	{
		#region - Fields -

		private HmLocalizer<T> m_embeddedLocalizer;
		private ResourceManager m_manager;

		#endregion

		public HmResXLocalizer(HmLocalizer<T> embeddedLocalizer)
		{
			if (embeddedLocalizer == null)
			{
				throw new ArgumentNullException("embeddedLocalizer");
			}
			m_embeddedLocalizer = embeddedLocalizer;
			CreateResourceManager();
		}

		#region - Properties -

		protected internal virtual ResourceManager Manager
		{
			get { return m_manager; }
		}

		public override String Language
		{
			get { return CultureInfo.CurrentUICulture.Name; }
		}

		internal HmLocalizer<T> EmbeddedLocalizer
		{
			get { return m_embeddedLocalizer; }
		}

		#endregion

		protected internal override void PopulateStringTable()
		{
		}

		protected internal virtual void CreateResourceManager()
		{
			if (m_manager != null)
			{
				m_manager.ReleaseAllResources();
			}
			m_manager = CreateResourceManagerCore();
		}

		public override String GetLocalizedString(T id)
		{
			String result = GetLocalizedStringFromResources(id);
			if (result == null)
			{
				result = m_embeddedLocalizer.GetLocalizedString(id);
			}
			return result;
		}

		protected String GetLocalizedStringFromResources(T id)
		{
			try
			{
				String resStr = String.Format("{0}.{1}", GetEnumTypeName(), id.ToString());
				return m_manager.GetString(resStr);
			}
			catch { return null; }
		}

		public override XmlDocument CreateXmlDocument()
		{
			return m_embeddedLocalizer.CreateXmlDocument();
		}

		public override HmLocalizer<T> CreateResXLocalizer()
		{
			return m_embeddedLocalizer.CreateResXLocalizer();
		}

		protected internal abstract ResourceManager CreateResourceManagerCore();
	}

	#endregion
}

namespace CuteAnt.Localization.Internal
{
	public static class HmLocalizierHelper<T> where T : struct
	{
		public static Dictionary<T, String> GetStringTable(HmLocalizer<T> localizer)
		{
			return localizer.StringTable;
		}
	}

	public abstract class ActiveLocalizerProvider<T> where T : struct
	{
		private HmLocalizer<T> defaultLocalizer;

		public ActiveLocalizerProvider(HmLocalizer<T> defaultLocalizer)
		{
			this.defaultLocalizer = defaultLocalizer;
			SetActiveLocalizerCore(defaultLocalizer);
		}

		protected internal HmLocalizer<T> DefaultLocalizer { get { return defaultLocalizer; } }

		public HmLocalizer<T> GetActiveLocalizer()
		{
			HmLocalizer<T> active = GetActiveLocalizerCore();
			if (active == null)
			{
				SetActiveLocalizerCore(DefaultLocalizer);
				return DefaultLocalizer;
			}
			else
			{
				return active;
			}
		}

		public void SetActiveLocalizer(HmLocalizer<T> localizer)
		{
			SetActiveLocalizerCore(localizer);
		}

		protected internal abstract HmLocalizer<T> GetActiveLocalizerCore();

		protected internal abstract void SetActiveLocalizerCore(HmLocalizer<T> localizer);
	}

	public class DefaultActiveLocalizerProvider<T> : ActiveLocalizerProvider<T> where T : struct
	{
		[ThreadStatic()]
		private static HmLocalizer<T> threadLocalizer;

		public DefaultActiveLocalizerProvider(HmLocalizer<T> defaultLocalizer)
			: base(defaultLocalizer)
		{
		}

		protected internal override HmLocalizer<T> GetActiveLocalizerCore()
		{
			return threadLocalizer;
		}

		protected internal override void SetActiveLocalizerCore(HmLocalizer<T> localizer)
		{
			threadLocalizer = localizer;
		}
	}
}
