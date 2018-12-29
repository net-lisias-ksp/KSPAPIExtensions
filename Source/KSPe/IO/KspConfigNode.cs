﻿/*
    This file is part of KSPe, a component for KSP API Extensions/L
    (C) 2018 Lisias T : http://lisias.net <support@lisias.net>

	KSPe API Extensions/L is double licensed, as follows:

	* SKL 1.0 : https://ksp.lisias.net/SKL-1_0.txt
	* GPL 2.0 : https://www.gnu.org/licenses/gpl-2.0.txt

	And you are allowed to choose the License that better suit your needs.

	KSPe API Extensions/L is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

	You should have received a copy of the SKL Standard License 1.0
    along with KSPe API Extensions/L. If not, see <https://ksp.lisias.net/SKL-1_0.txt>.

	You should have received a copy of the GNU General Public License 2.0
    along with KSPe API Extensions/L. If not, see <https://www.gnu.org/licenses/>.
    
*/

namespace KSPe.IO
{
	public class KspConfigNode : ReadableConfigNode
	{
		public KspConfigNode(string name) : base(name)
		{
			this.Path = GeneratePathname(name + ".cfg");
		}

		public KspConfigNode(string name, string filename) : base(name)
		{
			this.Path = GeneratePathname(filename);
		}

		public new KspConfigNode Load()
		{
			return (KspConfigNode)base.Load();
		}
		
		protected static string GeneratePathname(string filename)
		{
			string fn = System.IO.Path.Combine(KSPe.IO.File<object>.KSP_ROOTPATH, filename);
			return fn;
		}

	}
}