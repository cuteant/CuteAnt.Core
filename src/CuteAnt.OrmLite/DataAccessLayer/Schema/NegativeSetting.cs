namespace CuteAnt.OrmLite.DataAccessLayer
{
	/// <summary>���򹤳�����</summary>
	public class NegativeSetting
	{
		private bool _CheckOnly;
		private bool _NoDelete;

		/// <summary>�Ƿ�ֻ��顣������־������ִ�в���</summary>
		public bool CheckOnly
		{
			get { return _CheckOnly; }
			set { _CheckOnly = value; }
		}

		/// <summary>�Ƿ�ɾ������ִ�з��������ʱ�������漰ɾ�����ɾ���ֶεĲ����������С�</summary>
		public bool NoDelete
		{
			get { return _NoDelete; }
			set { _NoDelete = value; }
		}
	}
}