using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Data;
using System.Text;
//using System.Threading.Tasks;
using System.Data.SqlClient;
namespace MultiFaceRec
{
    public class CommonClasses
    {
       public SqlDataReader rdr = null;
       public DataTable dtable = new DataTable();
       public SqlConnection con = null;
       public SqlCommand cmd = null;
       public DataSet ds;
       public SqlDataAdapter da;
    }
}
