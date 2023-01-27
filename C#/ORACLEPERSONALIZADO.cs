using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using Oracle.DataAccess.Client;
using System.Xml;
using System.IO;
using System.Web.UI;
using System.Text;
using System.Drawing.Printing;
using System.Drawing.Imaging;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Drawing.Drawing2D;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CombexApp
{
    public class OracleFunc
    {
        private OracleFunc()
        {
        }

        public struct Parametros
        {
            public string Id;
            public string value;
        }

        public struct Valores
        {
            public string id;
            public string resp;
            public string motivo;
            public string fecha;
        }

        /// <summary>
        /// Funcion para obtener el string de conexion
        /// </summary>
        /// <param name="Id">Nombre de Usuario</param>
        /// <param name="Pass">Password del Usuario</param>
        /// <returns></returns>
        public static string CNNString(string Id, string Pass)
        {
            string oradb = "Data Source=" + CombexApp.Properties.Settings.Default.Esquema.ToString() + ";User Id=" + Id + ";Password=" + Pass + ";";

             return oradb;
        }

        /// <summary>
        /// Funcion que permite hacer consultas a la base de datos
        /// </summary>
        /// <param name="Querys">Sentencia SQL (Insert, Update y Delete)</param>        
        /// <returns></returns>
        public static DataSet RunQuery(string Querys, string Table)
        {
            DataSet ds = new DataSet(); // Dataset donde se devuelven los datos 

            //objeto de conexion y se toma la funcion que prepara el string de conexion
            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = CNNString(MySession.Current.Username, MySession.Current.Pass);

            //Command y DataAdapter para realizar la consulta
            OracleCommand cmd = new OracleCommand(Querys, conn);
            OracleDataAdapter da = new OracleDataAdapter();

            cmd.CommandTimeout = 0;

            da.SelectCommand = cmd;

            try
            {
                conn.Open(); //Abrir coneccion
                da.Fill(ds, Table); //Llenado de ds               
            }
            catch (Exception ex)
            {
                //En caso de error se devuelve null en dataset y se cierra la coneccion
                CreateLogFiles.ErrorLog("/Logs/", ex.Message.ToString() + "-" + Querys + "-RunQuery-App-" + MySession.Current.Username);
                ds = null;
            }
            finally
            {
                //Se cierra la coneccion en caso que todo finalice bien
                conn.Close();
                conn.Dispose();
            }

            return ds;
        }

        /// <summary>
        /// Funcion que permite hacer consultas a la base de datos
        /// </summary>
        /// <param name="Querys">Sentencia SQL (Insert, Update y Delete)</param>        
        /// <returns></returns>
        public static DataSet RunQuery(string Querys, string Table, ref OracleTransaction Tran, OracleConnection conn)
        {
            DataSet ds = new DataSet(); // Dataset donde se devuelven los datos 

            //Command y DataAdapter para realizar la consulta
            OracleCommand cmd = new OracleCommand(Querys, conn);
            OracleDataAdapter da = new OracleDataAdapter();

            cmd.CommandTimeout = 0;
            cmd.Transaction = Tran;

            da.SelectCommand = cmd;

            try
            {
                da.Fill(ds, Table); //Llenado de ds               
            }
            catch (Exception ex)
            {

                //En caso de error se devuelve null en dataset y se cierra la coneccion
                CreateLogFiles.ErrorLog("/Logs/", ex.Message.ToString() + "-RunQuery-App-" + MySession.Current.Username);
                ds = null;
            }
            finally
            {
                da.Dispose();
            }

            return ds;
        }
        /// <summary>
        /// Funcion que permite hacer consultas a la base de datos dispara excepciones.
        /// </summary>
        /// <param name="Querys">Sentencia SQL (Insert, Update y Delete)</param>        
        /// <returns></returns>
        public static DataSet RunQueryEx(string Querys, string Table, ref OracleTransaction Tran, OracleConnection conn)
        {
            DataSet ds = new DataSet(); // Dataset donde se devuelven los datos 

            //Command y DataAdapter para realizar la consulta
            OracleCommand cmd = new OracleCommand(Querys, conn);
            OracleDataAdapter da = new OracleDataAdapter();

            cmd.CommandTimeout = 0;
            cmd.Transaction = Tran;

            da.SelectCommand = cmd;

            try
            {
                da.Fill(ds, Table); //Llenado de ds               
            }
            catch (Exception ex)
            {

                //En caso de error se devuelve null en dataset y se cierra la coneccion
                CreateLogFiles.ErrorLog("/Logs/", ex.Message.ToString() + "-RunQuery-App-" + MySession.Current.Username);
                ds = null;
                throw;
            }
            finally
            {
                da.Dispose();
            }

            return ds;
        }


        //Funcion para llamar funciones de oracle
        //public static object CallFuntion(string Querys, List<OracleFunc.Parametros> Param)
        //{
        //    //Objeto de conexion y se toma la funcion que prepara el string de conexion
        //    OracleConnection conn = new OracleConnection();
        //    conn.ConnectionString = CNNString(MySession.Current.Username, MySession.Current.Pass);
        //    object res = new object();


        //    conn.Open();
        //    OracleTransaction OraTran;
        //    OracleCommand cmd;
        //    OraTran = conn.BeginTransaction();

        //    try
        //    {
        //        cmd = new OracleCommand(Querys, conn);
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        cmd.CommandTimeout = 0;

        //        foreach (var item in Param)
        //        {

        //            OracleParameter inval = new OracleParameter(item.Id, OracleDbType.Varchar2);
        //            inval.Direction = ParameterDirection.Input;
        //            inval.Value = item.value;
        //            cmd.Parameters.Add(inval);

        //        }

        //        OracleParameter retval = new OracleParameter("RETURN", OracleDbType.Int32,5);
        //        retval.Direction = ParameterDirection.ReturnValue;
        //        cmd.Parameters.Add(retval);

        //        cmd.ExecuteNonQuery();

        //        OraTran.Commit();

        //      res =  retval.Value.ToString();
        //    }
        //    catch (Exception ex)
        //    {
        //        OraTran.Rollback();
        //    }
        //    finally
        //    {
        //        conn.Close();
        //        conn.Dispose();
        //    }
        //    return res.ToString();
        //}

        //Funcion que permite hacer un Insert, Detele o Update

        /// <summary>
        /// Funcion para ejecutar Insert, Delete o Update
        /// </summary>
        /// <param name="Query">Sentencia SQL (Insert, Update y Delete)</param>
        /// <param name="Querys">Listas de sentencias SQL (Insert, Update y Delete)</param>
        /// <returns></returns>
        /// 
        public static string GetToken()
        {
            //solicita token
            dynamic token;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://wservices.OracleFunc.com.gt:50443/");
                //string jsonString = "{user:JQUINONES,pass:Jorge,2020}";

                dynamic product = new JObject();
                product.user = "WEBPAGE";
                product.pass = "WEBPAGE";
                
                var json = new StringContent(
                Convert.ToString(product),
                Encoding.UTF8,
                "application/json");
                //var result = await client.PostAsync("/api/User", content);
                var task = Task.Run(() => client.PostAsync("/api/User", json));
                var taskRead = Task.Run(() => task.Result.Content.ReadAsStringAsync());
                token = JsonConvert.DeserializeObject<dynamic>(taskRead.Result);
                //Console.WriteLine(resultContent);
            }
            //solicita token
            return token.data.token;
        }
        public static bool RunTransaction(string Query = null, List<string> Querys = null)
        {
            HttpContext.Current.Session["DB_EXCEPTION"] = null;
            //Objeto de conexion y se toma la funcion que prepara el string de conexion
            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = CNNString(MySession.Current.Username, MySession.Current.Pass);

            bool result = false;

            if (Query != null || Querys != null)
            {
                //Se abre la coneccion y se crea el objeto transaction
                conn.Open();
                OracleTransaction OraTran;
                OracleCommand cmd;

                OraTran = conn.BeginTransaction(); //Se inicia la transaccion

                try
                {
                    //Si es un solo query
                    if (Query != null && Querys == null)
                    {
                        cmd = new OracleCommand(Query, conn);
                        cmd.CommandTimeout = 0;
                        cmd.Transaction = OraTran;
                        cmd.ExecuteNonQuery();
                    }
                    else if (Query == null && Querys != null) 
                    {                        
                        foreach (var item in Querys)
                        {                            
                            cmd = new OracleCommand(item.ToString(), conn);
                            cmd.CommandTimeout = 0;
                            cmd.Transaction = OraTran;
                            cmd.ExecuteNonQuery();
                        }                        
                    }

                    OraTran.Commit();
                    result = true;
                }
                catch (Exception ex)
                {
                    //MValdez 2016-Sep-14
                    //Se guarda en la session el mensaje de la excepcion si ocurre algún problema.
                    HttpContext.Current.Session["DB_EXCEPTION"] = ex.Message;
                    
                    OraTran.Rollback();
                    result = false;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }

            }

            return result;
        }

		internal static string AddParam(object uPD_SAB_OPER_LECTURAS_, List<Parametros> param)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Funcion Para Ejecutar un Procedimiento Almacenado
		/// </summary>
		/// <param name="Query">Nombre del procedimiento almacenado</param>
		/// <param name="Parameters">Parametros que recibe el procedimiento almacenado</param>
		/// <returns></returns>
		public static bool RunSp(string Query, List<OracleFunc.Parametros> Parameters)
        {
            HttpContext.Current.Session["DB_EXCEPTION"] = null;
            //Objeto de conexion y se toma la funcion que prepara el string de conexion
            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = CNNString(MySession.Current.Username, MySession.Current.Pass);

            bool result = false;

            try
            {
                //Se abre la coneccion y se crea el objeto transaction
                conn.Open();
                OracleCommand cmd;

                cmd = new OracleCommand(Query, conn);

                //Se agregan los parametros
                foreach (var item in Parameters)
                {
                    cmd.Parameters.Add(item.Id,item.value);
                }
                cmd.CommandTimeout = 0;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.ExecuteNonQuery();

                result = true;
            }
            catch (Exception ex )
            {
                //MValdez 2016-Sep-14
                //Se guarda en la session el mensaje de la excepcion si ocurre algún problema.
              
                HttpContext.Current.Session["DB_EXCEPTION"] = ex.Message;
                result = false;
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
            return result;
        }


        /// <summary>
        /// Funcion Para Ejecutar un Procedimiento Almacenado Enviando parametros con Tipos Oracle.
        /// </summary>
        /// <param name="Query">Nombre del procedimiento almacenado</param>
        /// <param name="Parameters">Parametros que recibe el procedimiento almacenado</param>
        /// <returns></returns>
        public static bool RunSpOracleParams(string Query, List<OracleParameter> Parameters)
        {
                HttpContext.Current.Session["DB_EXCEPTION"] = null;
            //Objeto de conexion y se toma la funcion que prepara el string de conexion
            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = CNNString(MySession.Current.Username, MySession.Current.Pass);

            bool result = false;

            try
            {
                //Se abre la coneccion y se crea el objeto transaction
                conn.Open();
                OracleCommand cmd;

                cmd = new OracleCommand(Query, conn);

                //Se agregan los parametros
                foreach (var item in Parameters)
                {
                    cmd.Parameters.Add(item);
                }
                cmd.CommandTimeout = 0;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.ExecuteNonQuery();

                result = true;
            }
            catch (Exception ex)
            {
                //MValdez 2016-Sep-14
                //Se guarda en la session el mensaje de la excepcion si ocurre algún problema.
              
                HttpContext.Current.Session["DB_EXCEPTION"] = ex.Message;
                result = false;
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
            return result;
        }

        
        /// <summary>
        /// Funcion para validar el usuario y password
        /// </summary>
        /// <param name="Id">Nombre de Usuario</param>
        /// <param name="Pass">Password del Usuario</param>
        /// <returns></returns>
        public static bool ValidateUser(string Id, string Pass)
        {
            //objeto de conexion y se toma la funcion que prepara el string de conexion
            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = CNNString(Id, Pass);
            

            try
            {
                //Se valida si la coneccion se pudo abrir, SI = true NO = false
                conn.Open();

                //Si la coneccion es valida se almancenan en cookies el Usuario y la Contraseña
                MySession.Current.Pass = Pass;
                MySession.Current.Username = Id.ToUpper();

                return true;
            }
            catch (Exception ex)
            {
                //Si no se puede abrir la coneccion se devuelve false
                CreateLogFiles.ErrorLog("/Logs/",ex.Message.ToString()+"-"+Id);
                return false;
            }
            finally
            {                
                conn.Close(); //Cerrar la coneccion
            }
        }

        /// <summary>
        /// Funcion para validar el NIT
        /// </summary>
        /// <param name="NIT">Ingresar el numero de NIT con guion "-"</param>
        /// <returns></returns>
        public static bool ValidaNIT(string NIT)
        {
            int Pos;
            string Correlativo;
            string DigitoVerificador;
            int Factor;
            int Suma = 0;
            int Valor = 0;
            int X;
            double Xmod11;
            string S = string.Empty;

            bool Resp = false;

            try
            {
                Pos = NIT.IndexOf("-");
                if (Pos != 0)
                {
                    Correlativo = NIT.Substring(0, Pos);

                    DigitoVerificador = NIT.Substring(0, Pos + 1);
                    Factor = Correlativo.Length + 1;

                    for (X = 0; X < NIT.IndexOf("-") -1; X++)
                    {
                        Valor = int.Parse(NIT.Substring(X, 1));
                        Suma += (Valor * Factor);
                        Factor -= 1;
                    }

                    Xmod11 = (11 - (Suma % 11)) % 11;

                    S = Xmod11.ToString();

                    if ((Xmod11 == 10 && DigitoVerificador == "K") || (S == DigitoVerificador))
                    {
                        Resp = true;
                    }
                }
            }
            catch (Exception)
            {
                Resp = false;
            }
            return Resp;
        }

        #region "Serializar"

        /// <summary>
        /// Serializa una imagen y retorna un XML
        /// </summary>
        /// <param name="imagen">Imagen a serializar</param>
        /// <returns></returns>
        public static XmlNode ImagenToXMLNode(Image imagen)
        {
            MemoryStream oStream = new MemoryStream();
            XmlDocument oDom = new XmlDocument();
            MemoryStream mResult = new MemoryStream();
            long LenData = 0;
            byte[] Buffer;
            BinaryReader oBinaryReader;
            XmlTextWriter oXMLTextWriter;
            StreamReader oStreamReader;
            string StrResult;

            //Verifico si existe la imagen a serializar
            if (imagen != null)
            {
                //Se graba en Stream para almacenar la imagen en formato binario
                //Se conserva el formato de la imagen
                ImageFormat imgF;
                imgF = imagen.RawFormat;
                imagen.Save(oStream, imgF);

                oStream.Position = 0;

                LenData = oStream.Length - 1;

                //Verifico la longitud de los datos a serializar
                if (LenData > 0)
                {
                    Buffer = new byte[Convert.ToInt32(LenData)];

                    //Leo los datos binarios
                    oBinaryReader = new BinaryReader(oStream, System.Text.Encoding.UTF8);
                    oBinaryReader.Read(Buffer, 0, Buffer.Length);

                    //Creo  XMLTextWriter y agrego nodo con la imagen
                    oXMLTextWriter = new XmlTextWriter(mResult, System.Text.Encoding.UTF8);
                    oXMLTextWriter.WriteStartDocument();
                    oXMLTextWriter.WriteStartElement("BinaryData");
                    oXMLTextWriter.WriteBase64(Buffer, 0, Buffer.Length);
                    oXMLTextWriter.WriteEndElement();
                    oXMLTextWriter.WriteEndDocument();
                    oXMLTextWriter.Flush();

                    //Posiciono en 0 el resultado
                    mResult.Position = 0;

                    //Pasa el Stream a string y retorna
                    oStreamReader = new StreamReader(mResult, System.Text.Encoding.UTF8);
                    StrResult = oStreamReader.ReadToEnd();
                    oStreamReader.Close();

                    //Agrego nuevo nodo con imagen
                    oDom.LoadXml(StrResult);
                    return oDom.DocumentElement;
                }
                else
                {
                    //En caso de no existir datos retorno el XML con formato vacio
                    oDom.LoadXml("<BinaryData/>");
                    return oDom.DocumentElement.CloneNode(true);
                }
            }
            else
            {
                //no hay imagen devuelvo el XML Vacio
                oDom.LoadXml("<BinaryData/>");
                return oDom.DocumentElement.CloneNode(true);
            }

        }

        public static XmlNode ImagenToXMLNode(Image imagen, string nombre)
        {
            MemoryStream oStream = new MemoryStream();
            XmlDocument oDom = new XmlDocument();
            MemoryStream mResult = new MemoryStream();
            long LenData = 0;
            byte[] Buffer;
            BinaryReader oBinaryReader;
            XmlTextWriter oXMLTextWriter;
            StreamReader oStreamReader;
            string StrResult;

            //Verifico si existe la imagen a serializar
            if (imagen != null)
            {
                //Se graba en Stream para almacenar la imagen en formato binario
                //Se conserva el formato de la imagen
                ImageFormat imgF;
                imgF = imagen.RawFormat;
                imagen.Save(oStream, imgF);

                oStream.Position = 0;

                LenData = oStream.Length - 1;

                //Verifico la longitud de los datos a serializar
                if (LenData > 0)
                {
                    Buffer = new byte[Convert.ToInt32(LenData)];

                    //Leo los datos binarios
                    oBinaryReader = new BinaryReader(oStream, System.Text.Encoding.UTF8);
                    oBinaryReader.Read(Buffer, 0, Buffer.Length);

                    //Creo  XMLTextWriter y agrego nodo con la imagen
                    oXMLTextWriter = new XmlTextWriter(mResult, System.Text.Encoding.UTF8);
                    oXMLTextWriter.WriteStartDocument();
                    oXMLTextWriter.WriteStartElement("BinaryData");
                    oXMLTextWriter.WriteAttributeString("Nombre", nombre);
                    oXMLTextWriter.WriteBase64(Buffer, 0, Buffer.Length);
                    oXMLTextWriter.WriteEndElement();
                    oXMLTextWriter.WriteEndDocument();
                    oXMLTextWriter.Flush();

                    //Posiciono en 0 el resultado
                    mResult.Position = 0;

                    //Pasa el Stream a string y retorna
                    oStreamReader = new StreamReader(mResult, System.Text.Encoding.UTF8);
                    StrResult = oStreamReader.ReadToEnd();
                    oStreamReader.Close();

                    //Agrego nuevo nodo con imagen
                    oDom.LoadXml(StrResult);
                    return oDom.DocumentElement;
                }
                else
                {
                    //En caso de no existir datos retorno el XML con formato vacio
                    oDom.LoadXml("<BinaryData/>");
                    return oDom.DocumentElement.CloneNode(true);
                }
            }
            else
            {
                //no hay imagen devuelvo el XML Vacio
                oDom.LoadXml("<BinaryData/>");
                return oDom.DocumentElement.CloneNode(true);
            }

        }

        /// <summary>
        /// Serializa Archivos y retorna un XML
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static XmlNode MSToXMLNode(MemoryStream file)
        {
            MemoryStream oStream = new MemoryStream();
            XmlDocument oDom = new XmlDocument();
            MemoryStream mResult = new MemoryStream();
            long LenData = 0;
            byte[] Buffer;
            BinaryReader oBinaryReader;
            XmlTextWriter oXMLTextWriter;
            StreamReader oStreamReader;
            string StrResult;

            //Verifico si existe la imagen a serializar
            if (file != null)
            {
                //Se graba en Stream para almacenar la imagen en formato binario
                //Se conserva el formato de la imagen
                //ImageFormat imgF;
                //imgF = imagen.RawFormat;
                //imagen.Save(oStream, imgF);

                oStream = file;

                oStream.Position = 0;

                LenData = oStream.Length - 1;

                //Verifico la longitud de los datos a serializar
                if (LenData > 0)
                {
                    Buffer = new byte[Convert.ToInt32(LenData)];

                    //Leo los datos binarios
                    oBinaryReader = new BinaryReader(oStream, System.Text.Encoding.UTF8);
                    oBinaryReader.Read(Buffer, 0, Buffer.Length);

                    //Creo  XMLTextWriter y agrego nodo con la imagen
                    oXMLTextWriter = new XmlTextWriter(mResult, System.Text.Encoding.UTF8);
                    oXMLTextWriter.WriteStartDocument();
                    oXMLTextWriter.WriteStartElement("BinaryData");
                    oXMLTextWriter.WriteBase64(Buffer, 0, Buffer.Length);
                    oXMLTextWriter.WriteEndElement();
                    oXMLTextWriter.WriteEndDocument();
                    oXMLTextWriter.Flush();

                    //Posiciono en 0 el resultado
                    mResult.Position = 0;

                    //Pasa el Stream a string y retorna
                    oStreamReader = new StreamReader(mResult, System.Text.Encoding.UTF8);
                    StrResult = oStreamReader.ReadToEnd();
                    oStreamReader.Close();

                    //Agrego nuevo nodo con imagen
                    oDom.LoadXml(StrResult);
                    return oDom.DocumentElement;
                }
                else
                {
                    //En caso de no existir datos retorno el XML con formato vacio
                    oDom.LoadXml("<BinaryData/>");
                    return oDom.DocumentElement.CloneNode(true);
                }
            }
            else
            {
                //no hay imagen devuelvo el XML Vacio
                oDom.LoadXml("<BinaryData/>");
                return oDom.DocumentElement.CloneNode(true);
            }

        }

        public static XmlNode MSToXMLNode(MemoryStream file, string nombre)
        {
            MemoryStream oStream = new MemoryStream();
            XmlDocument oDom = new XmlDocument();
            MemoryStream mResult = new MemoryStream();
            long LenData = 0;
            byte[] Buffer;
            BinaryReader oBinaryReader;
            XmlTextWriter oXMLTextWriter;
            StreamReader oStreamReader;
            string StrResult;

            //Verifico si existe la imagen a serializar
            if (file != null)
            {
                //Se graba en Stream para almacenar la imagen en formato binario
                //Se conserva el formato de la imagen
                //ImageFormat imgF;
                //imgF = imagen.RawFormat;
                //imagen.Save(oStream, imgF);

                oStream = file;

                oStream.Position = 0;

                LenData = oStream.Length - 1;

                //Verifico la longitud de los datos a serializar
                if (LenData > 0)
                {
                    Buffer = new byte[Convert.ToInt32(LenData)];

                    //Leo los datos binarios
                    oBinaryReader = new BinaryReader(oStream, System.Text.Encoding.UTF8);
                    oBinaryReader.Read(Buffer, 0, Buffer.Length);

                    //Creo  XMLTextWriter y agrego nodo con la imagen
                    oXMLTextWriter = new XmlTextWriter(mResult, System.Text.Encoding.UTF8);
                    oXMLTextWriter.WriteStartDocument();
                    oXMLTextWriter.WriteStartElement("BinaryData");
                    oXMLTextWriter.WriteAttributeString("Nombre", nombre);
                    oXMLTextWriter.WriteBase64(Buffer, 0, Buffer.Length);
                    oXMLTextWriter.WriteEndElement();
                    oXMLTextWriter.WriteEndDocument();
                    oXMLTextWriter.Flush();

                    //Posiciono en 0 el resultado
                    mResult.Position = 0;

                    //Pasa el Stream a string y retorna
                    oStreamReader = new StreamReader(mResult, System.Text.Encoding.UTF8);
                    StrResult = oStreamReader.ReadToEnd();
                    oStreamReader.Close();

                    //Agrego nuevo nodo con imagen
                    oDom.LoadXml(StrResult);
                    return oDom.DocumentElement;
                }
                else
                {
                    //En caso de no existir datos retorno el XML con formato vacio
                    oDom.LoadXml("<BinaryData/>");
                    return oDom.DocumentElement.CloneNode(true);
                }
            }
            else
            {
                //no hay imagen devuelvo el XML Vacio
                oDom.LoadXml("<BinaryData/>");
                return oDom.DocumentElement.CloneNode(true);
            }

        }

        /// <summary>
        /// DesSerializa un XML y retorna un Stream
        /// </summary>
        /// <param name="Nodo"></param>
        /// <returns></returns>
        public static MemoryStream XMLNodeToMS(XmlNode Nodo)
        {
            int IntResult = 0;
            int IntPosition = 0;
            int LenBytes = 1024 * 1024; //1024KB - 1MB Lee bloques de 1MB
            byte[] myBytes = new byte[(LenBytes - 1 + 1)];
            MemoryStream oMem = new MemoryStream();
            XmlTextReader oXMLTextReader;
            bool NodeFound = false;
            //Dim oStreamReader As IO.StreamReader
            StreamWriter oStreamWriter;
            MemoryStream oTempMem = new MemoryStream();
            string nombre = "";

            try
            {
                //Cargo nodo de texto en Memory Stream
                // para almacenar el archivo temporalmente en bytes
                oStreamWriter = new StreamWriter(oTempMem, System.Text.Encoding.UTF8);
                oStreamWriter.Write(Nodo.OuterXml);
                oStreamWriter.Flush();
                oTempMem.Position = 0;

                //Cargo un xmlReader con el Memory Stream para leer la imágen almacenada
                oXMLTextReader = new XmlTextReader(oTempMem);

                //Busco el Nodo en Binario
                while (oXMLTextReader.Read())
                {
                    if (oXMLTextReader.Name == "BinaryData")
                    {
                        NodeFound = true;
                        break;
                    }
                }

                //Verifico si se encontró el Nodo con el archivo
                if (NodeFound)
                {

                    if (oXMLTextReader.HasAttributes)
                    {
                        nombre = oXMLTextReader.GetAttribute("Nombre");
                    }

                    //Lo encontro, me muevo a la Posicion Inicial del Stream para leerlo
                    IntPosition = 0;

                    //Intento Leer
                    IntResult = oXMLTextReader.ReadBase64(myBytes, 0, LenBytes);
                    while (IntResult > 0)
                    {

                        //Escribe datos

                        oMem.Write(myBytes, 0, IntResult);

                        //Limpio el array
                        Array.Clear(myBytes, 0, LenBytes);

                        //Leo nuevamente
                        IntResult = oXMLTextReader.ReadBase64(myBytes, 0, LenBytes);

                    }
                    try
                    {
                        //Intento crear la Imagen y retornarla si no devuelvo Nothing
                        //System.Drawing.Image img;
                        //img = System.Drawing.Bitmap.FromStream(oMem, true, true);
                        //if ((nombre != null) && (nombre.Length > 0))
                        //{
                        //    img.Tag = nombre;
                        //}

                        return oMem;
                    }
                    catch
                    {
                        return null;
                    }
                }
                else
                {
                    //No encontró el nodo de archivo
                    return null;
                }

            }
            catch (Exception )
            {
                //Ocurrio un error no contemplado Retorno Nothing
                return null;
            }
        }               

        /// <summary>
        ///DesSerializa XML y retorna la Imágen
        /// </summary>
        /// <param name="Nodo">Nodo de XML</param>
        /// <returns></returns>
        public static System.Drawing.Image XMLNodeToImage(XmlNode Nodo)
        {
            int IntResult = 0;
            int IntPosition = 0;
            int LenBytes = 1024 * 1024; //1024KB - 1MB Lee bloques de 1MB
            byte[] myBytes = new byte[(LenBytes - 1 + 1)];
            MemoryStream oMem = new MemoryStream();
            XmlTextReader oXMLTextReader;
            bool NodeFound = false;
            //Dim oStreamReader As IO.StreamReader
            StreamWriter oStreamWriter;
            MemoryStream oTempMem = new MemoryStream();
            string nombre = "";

            try
            {
                //Cargo nodo de texto en Memory Stream
                // para almacenar la imagen temporalmente en bytes
                oStreamWriter = new StreamWriter(oTempMem, System.Text.Encoding.UTF8);
                oStreamWriter.Write(Nodo.OuterXml);
                oStreamWriter.Flush();
                oTempMem.Position = 0;

                //Cargo un xmlReader con el Memory Stream para leer la imágen almacenada
                oXMLTextReader = new XmlTextReader(oTempMem);

                //Busco el Nodo en Binario
                while (oXMLTextReader.Read())
                {
                    if (oXMLTextReader.Name == "BinaryData")
                    {
                        NodeFound = true;
                        break;
                    }
                }

                //Verifico si se encontró el Nodo con la imagen
                if (NodeFound)
                {

                    if (oXMLTextReader.HasAttributes)
                    {
                        nombre = oXMLTextReader.GetAttribute("Nombre");
                    }

                    //Lo encontro, me muevo a la Posicion Inicial del Stream para leerlo
                    IntPosition = 0;

                    //Intento Leer
                    IntResult = oXMLTextReader.ReadBase64(myBytes, 0, LenBytes);
                    while (IntResult > 0)
                    {

                        //Escribe datos

                        oMem.Write(myBytes, 0, IntResult);

                        //Limpio el array
                        Array.Clear(myBytes, 0, LenBytes);

                        //Leo nuevamente
                        IntResult = oXMLTextReader.ReadBase64(myBytes, 0, LenBytes);

                    }
                    try
                    {
                        //Intento crear la Imagen y retornarla si no devuelvo Nothing
                        System.Drawing.Image img;
                        img = System.Drawing.Bitmap.FromStream(oMem, true, true);
                        if ((nombre != null) && (nombre.Length > 0))
                        {
                            img.Tag = nombre;
                        }
                        return img;
                    }
                    catch
                    {
                        return null;
                    }
                }
                else
                {
                    //No encontró el nodo de imágen
                    return null;
                }

            }
            catch (Exception )
            {
                //Ocurrio un error no contemplado Retorno Nothing
                return null;
            }
        }

        public static byte[] imageToByteArray(System.Drawing.Image Imagein)
        {
            MemoryStream ms = new MemoryStream();
            Imagein.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

            return ms.ToArray();
        }

        public static System.Drawing.Image byteArrayToImage(byte byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            System.Drawing.Image returnimage;

            returnimage = System.Drawing.Image.FromStream(ms);

            return returnimage;
        }

        
        #endregion

        /// <summary>
        /// Funcion para encontrar el Tipo de Papel Personalizado en la impresora a donde se envia el documento
        /// </summary>
        /// <param name="PrintName">Nombre de la impresora</param>
        /// <param name="PaperName">Nombre del tipo de papel personalizado</param>
        /// <returns></returns>
        public static int PaperSize(string PrintName, string PaperName)
        {
            System.Drawing.Printing.PrinterSettings PrintSettings = new System.Drawing.Printing.PrinterSettings();
            System.Drawing.Printing.PaperSize PaperSize = new System.Drawing.Printing.PaperSize();
            PrintSettings.PrinterName = PrintName;
            for (int i = 0; i < PrintSettings.PaperSizes.Count; i++)
            {
                if (PrintSettings.PaperSizes[i].PaperName == PaperName)
                {
                    PaperSize = PrintSettings.PaperSizes[i];
                }
            }

            return PaperSize.RawKind;
        }

        /// <summary>
        /// Agrega los parametros a las sentencias SQL
        /// </summary>
        /// <param name="RxQuery">Sentencia SQL al tratar</param>
        /// <param name="Param">Lista de Parametros que se van a agregar al Query</param>
        /// <returns></returns>
        public static string AddParam(string RxQuery,List<Parametros> Param)
        {
            foreach (var item in Param)
            {
                RxQuery = RxQuery.Replace(item.Id, item.value.Sanitize());
            }

            return RxQuery;
            
        }

        /// <summary>
        /// Buscar el nombre de la impresora segun el codigo
        /// </summary>
        /// <param name="Impr_Id">Id de la impresora</param>
        /// <returns></returns>
        public static string BuscarImpresora(int Impr_Id)
        {
            string Result = "";
            var dt = RunQuery(Mov_Impo.Mov.SEL_BUSCA_RUTA_IMPR.Replace("_Impr_Id_", Impr_Id.ToString()), "Print").Tables["Print"];

            if (dt.Rows.Count > 0)
            {
                Result = dt.Rows[0]["IMPR_RUTA"].ToString();
            }            

            return Result;
        }

        public static string Menu(short MENT_ID)
        {
            string result = "";
            DataTable ds = new DataTable();          

            List<Parametros> Param = new List<Parametros>();
            Parametros Code = new Parametros();

            //Se agregan los parametros al Query
                Code.Id = "_var1_";
                Code.value = MENT_ID.ToString();
                Param.Add(Code);

                Code.Id = "_var2_";
                Code.value = MySession.Current.Username.ToString();
                Param.Add(Code);
            
            //Se realiza la consilta
            ds = RunQuery(AddParam(Resource_Combex.SEL_GENE_MEND.ToString(),Param), "Mens").Tables["Mens"];

            Param.Clear();

            //Se valida si el query dio resultados
            if (ds.Rows.Count > 0)
            {
                var qmenu = from a in ds.AsEnumerable()
                            select new { 
                                Seg_Gene_Id = a.Field<decimal>("SEG_GENE_ID"),
                                Mend_Id = a.Field<short>("MEND_ID"),
                                Mend_Nombre = a.Field<string>("MEND_NOMBRE"),
                                Mend_Enlace = a.Field<string>("MEND_ENLACE"),
                                Mend_Padre = a.Field<Nullable<short>>("MEND_PADRE"),
                                Mend_Orden = a.Field<short>("MEND_ORDEN")
                            };

                var q = from a in qmenu
                        group a by new
                        {
                            a.Mend_Id,
                            a.Mend_Nombre,
                            a.Mend_Enlace,
                            a.Mend_Padre,
                            a.Mend_Orden
                        } into grc
                        select new
                        {
                            Seg_Gene_Id = grc.ToList(),
                            Mend_Id = grc.Key.Mend_Id,
                            Mend_Nombre = grc.Key.Mend_Nombre,
                            Mend_Enlace = grc.Key.Mend_Enlace,
                            Mend_Padre = grc.Key.Mend_Padre,
                            Mend_Orden = grc.Key.Mend_Orden
                        };

                List<Menud> lst = new List<Menud>();
           
                foreach (var item in q)
                {
                    List<decimal> lt = new List<decimal>();
                    foreach (var it in item.Seg_Gene_Id)
                    {
                        lt.Add(it.Seg_Gene_Id);
                    }

                    lst.Add(new Menud { 
                        Seg_Gene_Id = lt.ToList(),
                        Mend_Id = item.Mend_Id,
                        Mend_Nombre = item.Mend_Nombre,
                        Mend_Enlace = item.Mend_Enlace,
                        Mend_Padre = item.Mend_Padre,
                        Mend_Orden = item.Mend_Orden    
                    });                  
                }
                
                //foreach (var item in qmenu)
                //{   

                //    lst.Add(new Menud
                //    {
                //        Seg_Gene_Id = item.Seg_Gene_Id,
                //        Mend_Id = item.Mend_Id,
                //        Mend_Nombre = item.Mend_Nombre,
                //        Mend_Enlace = item.Mend_Enlace,
                //        Mend_Padre = item.Mend_Padre,
                //        Mend_Orden = item.Mend_Orden                  
                //    });
                //}

                //Se llama a la funcion recursiva que dibuja los menus
                result = Items(null, lst);

                lst.Clear();

            }
            
            return result.ToString();
        }

        //Función Recursiva que dibuja los menus
        private static string Items(int? parent, List<Menud> p_lst)
        {            
            
            StringWriter stringWriter = new StringWriter();
            HtmlTextWriter result = new HtmlTextWriter(stringWriter);

            var lst = p_lst;

            if (parent == null) //Se validan los menus que no tiene padre
            {
                lst = lst.Where(x => x.Mend_Padre == null).ToList();
            }
            else //Se validan los menus que tiene padre
            {
                lst = lst.Where(x => x.Mend_Padre == parent).ToList();
            }


            if(parent == null)
            {
                result.AddAttribute(HtmlTextWriterAttribute.Class, "nav menu");
            }

            result.RenderBeginTag(HtmlTextWriterTag.Ul);//Begin ul

            if (parent == null)
            {
                //El menu de Inicio default para todos
                result.AddAttribute(HtmlTextWriterAttribute.Class, "current");
                result.RenderBeginTag(HtmlTextWriterTag.Li);
                result.AddAttribute(HtmlTextWriterAttribute.Class, "parent");
                //result.AddAttribute(HtmlTextWriterAttribute.Href, "?");
                result.AddAttribute("data-bind", "with: user");
                result.AddAttribute("onclick", "ui.closeApp()"); 
                result.RenderBeginTag(HtmlTextWriterTag.A);
                result.RenderBeginTag(HtmlTextWriterTag.Span);
                result.Write("Inicio");
                result.RenderEndTag();
                result.RenderEndTag();
                result.RenderEndTag();
            }

            foreach (var item in lst)
            {
                
                result.RenderBeginTag(HtmlTextWriterTag.Li); //Begin li

                var hijos = from a in p_lst
                            where a.Mend_Padre == item.Mend_Id
                            select a;

                if (hijos.Count() > 0)
                {
                    result.AddAttribute(HtmlTextWriterAttribute.Class, "parent");
                }
                
                if(item.Mend_Enlace.Trim() != "#" && item.Mend_Enlace.Trim() != "" && item.Mend_Enlace.Trim() != "?")
                {
                    string Ids = "";
                    foreach (var it in item.Seg_Gene_Id)
                    {
                        Ids = Ids + it.ToString() + "-";
                    }


                    result.AddAttribute("data-bind", "with: user");
                    result.AddAttribute("onclick", "ui.openApp('" + item.Mend_Nombre + "','" + item.Mend_Enlace + "', '" + Ids + "')");                
                }
                result.RenderBeginTag(HtmlTextWriterTag.A); //Begin a

                result.RenderBeginTag(HtmlTextWriterTag.Span); //Begin span
                result.Write(item.Mend_Nombre);
                result.RenderEndTag(); //End span

                result.RenderEndTag(); //End a

                //Inicia la recursividad
                if (hijos.Count() > 0)
                {
                    result.Write(Items(item.Mend_Id, p_lst));
                }
                                
                result.RenderEndTag(); //End li
            }

            result.RenderEndTag(); //End ul
            

            return result.InnerWriter.ToString();
        }

        public static string Valid(string Gaf_Cod, string Gaf_Anio, string Type)
        {
            string result = "";
            
            List<Parametros> Param = new List<Parametros>();
            
            //Se agregan los parametros al Query
            Param.Add(new Parametros { Id = "_Gaf_Cod_", value = Gaf_Cod });
            Param.Add(new Parametros { Id = "_Gaf_Anio_", value = Gaf_Anio });

            //Se realiza la consilta            
            var ds = RunQuery(AddParam(CombexApp.ServicioCliente.Gafetes.SEL_GAF_VAL,Param), "Vals").Tables["Vals"];

            Param.Clear();

            //Se valida si el query dio resultados
            if (ds.Rows.Count > 0)
            {
                var qmenu = from a in ds.AsEnumerable()
                            select new
                            {
                                Val_Id = a.Field<short>("VALI_ID"),
                                Val_Desc = a.Field<string>("VALI_DESC"),
                                Val_Parent = a.Field<Nullable<short>>("VALI_PARENT"),
                                Val_Valida = a.Field<string>("VALI_VALIDA"),
                                GafVal_Resp = a.Field<string>("GAVA_RESP"),
                                GafVal_Motivo = a.Field<string>("GAVA_MOTIVO"),
                                GafVal_Vence = a.Field<string>("GAVA_VENCE")
                            };

                List<Val> lst = new List<Val>();

                foreach (var item in qmenu)
                {
                    lst.Add(new Val
                    {
                        Val_Id = item.Val_Id,
                        Val_Desc = item.Val_Desc,
                        Val_Parent = item.Val_Parent,
                        Val_Valida = item.Val_Valida,
                        GafVal_Resp = item.GafVal_Resp,
                        GafVal_Motivo = item.GafVal_Motivo,
                        GafVal_Vence = item.GafVal_Vence
                    });
                }

                //Se llama a la funcion recursiva que dibuja los menus
                result = Items_Valid(null, lst, Type);

                lst.Clear();

            }

            return result.ToString();
        }

        public static string Valid2(string Cli_Id, string Anio, string Type)
        {
            string result = "";

            List<Parametros> Param = new List<Parametros>();

            //Se agregan los parametros al Query
            Param.Add(new Parametros { Id = "_Cli_Id_", value = Cli_Id });
            Param.Add(new Parametros { Id = "_Gav2_Anio_", value = Anio });

            //Se realiza la consilta            
            var ds = RunQuery(AddParam(CombexApp.ServicioCliente.Gafetes.SEL_GAF_VAL2, Param), "Vals").Tables["Vals"];

            Param.Clear();

            //Se valida si el query dio resultados
            if (ds.Rows.Count > 0)
            {
                var qmenu = from a in ds.AsEnumerable()
                            select new
                            {
                                Val_Id = a.Field<short>("VALI_ID"),
                                Val_Desc = a.Field<string>("VALI_DESC"),
                                Val_Parent = a.Field<Nullable<short>>("VALI_PARENT"),
                                Val_Valida = a.Field<string>("VALI_VALIDA"),
                                GafVal_Resp = a.Field<string>("GAV2_RESP"),
                                GafVal_Motivo = a.Field<string>("GAV2_MOTIVO"),
                                GafVal_Vence = a.Field<string>("GAVA_VENCE")
                            };

                List<Val> lst = new List<Val>();

                foreach (var item in qmenu)
                {
                    lst.Add(new Val
                    {
                        Val_Id = item.Val_Id,
                        Val_Desc = item.Val_Desc,
                        Val_Parent = item.Val_Parent,
                        Val_Valida = item.Val_Valida,
                        GafVal_Resp = item.GafVal_Resp,
                        GafVal_Motivo = item.GafVal_Motivo,
                        GafVal_Vence = item.GafVal_Vence
                    });
                }

                //Se llama a la funcion recursiva que dibuja los menus
                result = Items_Valid(null, lst, Type);

                lst.Clear();

            }

            return result.ToString();
        }

        //Función Recursiva que dibuja las Validaciones
        private static string Items_Valid(int? parent, List<Val> p_lst, string Type)
        {

            StringWriter stringWriter = new StringWriter();
            HtmlTextWriter result = new HtmlTextWriter(stringWriter);

            var lst = p_lst;

            if (parent == null) //Se validan los menus que no tiene padre
            {
                lst = lst.Where(x => x.Val_Parent == null).ToList();
            }
            else //Se validan los menus que tiene padre
            {
                lst = lst.Where(x => x.Val_Parent == parent).ToList();
            }

            if (parent == null)
            {
                result.AddAttribute(HtmlTextWriterAttribute.Class, "");
            }

            result.AddAttribute(HtmlTextWriterAttribute.Style, "width:100%;");
            //result.RenderBeginTag(HtmlTextWriterTag.Table);//Begin Table
                        
            //if (parent == null)
            //{
            //    //El menu de Inicio default para todos
            //    result.AddAttribute(HtmlTextWriterAttribute.Class, "");
            //    result.RenderBeginTag(HtmlTextWriterTag.Li);
            //    result.AddAttribute(HtmlTextWriterAttribute.Class, "");
                
            //    //result.AddAttribute("data-bind", "with: user");
            //    //result.AddAttribute("onclick", "ui.closeApp()");
            //    result.RenderBeginTag(HtmlTextWriterTag.A);
            //    result.RenderBeginTag(HtmlTextWriterTag.Span);
            //    result.Write("Documentacion Requerida");
            //    result.RenderEndTag();
            //    result.RenderEndTag();
            //    result.RenderEndTag();
            //}

            foreach (var item in lst)
            {

                //result.AddAttribute(HtmlTextWriterAttribute.Id,item.Val_Id.ToString());                
                //result.RenderBeginTag(HtmlTextWriterTag.Tr); //Begin li
                //result.RenderBeginTag(HtmlTextWriterTag.Td);
                    
                var hijos = from a in p_lst
                            where a.Val_Parent == item.Val_Id
                            select a;

                //if (hijos.Count() > 0)
                //{
                //    //result.AddAttribute(HtmlTextWriterAttribute.Class, "parent");
                //}

                //if (item.Mend_Enlace.Trim() != "#" && item.Mend_Enlace.Trim() != "" && item.Mend_Enlace.Trim() != "?")
                //{
                    //result.AddAttribute("data-bind", "with: user");
                    //result.AddAttribute("onclick", "ui.openApp('" + item.Mend_Nombre + "','" + item.Mend_Enlace + "', '" + item.Seg_Gene_Id + "')");
                //}
                
                List<Parametros> Param = new List<Parametros>();
                
                if (item.Val_Valida == "S")
                {
                    Param.Add(new Parametros { Id = "_Val_Id_", value = item.Val_Id.ToString() });
                    Param.Add(new Parametros { Id = "_Val_Desc_", value = item.Val_Parent != null ? "<blockquote>" + item.Val_Desc + "</blockquote>" : item.Val_Desc });
                    Param.Add(new Parametros { Id = "_Val_Resp_", value = item.GafVal_Resp });
                    Param.Add(new Parametros { Id = "_Val_Motivo_", value = item.GafVal_Motivo });
                    Param.Add(new Parametros { Id = "_Val_FechaVenc_", value = item.GafVal_Vence });
                    Param.Add(new Parametros { Id = "_Type_", value = Type == "R" || Type == "A" ? "disabled" : "" });
                    Param.Add(new Parametros { Id = "_Val_Tp_", value = Type == "R" || Type == "A" ? "N" : "S" });

                    result.Write(OracleFunc.AddParam(CombexApp.ServicioCliente.Gafetes.HTML_VALID_TBL, Param).ToString());                    
                }
                else
                {                    
                    result.Write("<tr><td style='height:39px;' colspan='4'>" + item.Val_Desc + "</td></tr>");               
                }
                Param.Clear();

                //Inicia la recursividad
                if (hijos.Count() > 0)
                {                    
                    result.Write(Items_Valid(item.Val_Id, p_lst, Type));                    
                }
                //result.RenderEndTag(); // End Td
                //result.RenderEndTag(); //End tr
            }

            //result.RenderEndTag(); //End Table
            
            return result.InnerWriter.ToString();
        }
        
        public static string Role(string Id, string Nombre, string Descripcion)
        {
            StringWriter stringWriter = new StringWriter();
            HtmlTextWriter result = new HtmlTextWriter(stringWriter);

            result.AddAttribute(HtmlTextWriterAttribute.Class, "roles ui-corner-all");
            result.AddAttribute(HtmlTextWriterAttribute.Id, "li-" + Id);
            result.RenderBeginTag(HtmlTextWriterTag.Li); //Begin Li

            result.AddAttribute(HtmlTextWriterAttribute.Style, "width:100%; height:110px; color:white");
            result.RenderBeginTag(HtmlTextWriterTag.Table); //Begin Table

            result.RenderBeginTag(HtmlTextWriterTag.Tr); //Begin Tr
            result.RenderBeginTag(HtmlTextWriterTag.Td); //Begin Td
            result.Write("Nombre:");
            result.RenderEndTag(); //End Td
            result.RenderBeginTag(HtmlTextWriterTag.Td); //Begin Td
            result.Write(Nombre);
            result.RenderEndTag(); //End Td                
            result.RenderEndTag();  //End Tr

           

            result.RenderBeginTag(HtmlTextWriterTag.Tr); //Begin Tr
            result.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
            result.AddAttribute(HtmlTextWriterAttribute.Align, "center");
            result.RenderBeginTag(HtmlTextWriterTag.Td); //Begin Td 

            if (MySession.Current.Acciones.Contains("M"))
            {
                result.AddAttribute("data-id", Id);
                result.AddAttribute("data-nombre", Nombre);
                result.AddAttribute("data-desc", Descripcion);
                result.AddAttribute(HtmlTextWriterAttribute.Class, "btn_editarr");
                result.AddAttribute(HtmlTextWriterAttribute.Type, "radio");
                result.AddAttribute(HtmlTextWriterAttribute.Title, "Editar");
                result.AddAttribute(HtmlTextWriterAttribute.Name, "Group");
                result.AddAttribute(HtmlTextWriterAttribute.Id, "rdb_" + Id);
                result.RenderBeginTag(HtmlTextWriterTag.Input);
                result.RenderEndTag();
                result.AddAttribute("class", "lbl_conf");
                result.AddAttribute(HtmlTextWriterAttribute.For, "rdb_" + Id);
                result.RenderBeginTag(HtmlTextWriterTag.Label);
                result.Write("Editar");
                result.RenderEndTag();
            }

            if (MySession.Current.Acciones.Contains("E"))
            {
                result.AddAttribute("data-id", Id);
                result.AddAttribute(HtmlTextWriterAttribute.Class, "btn_eliminar");
                result.AddAttribute(HtmlTextWriterAttribute.Type, "checkbox");
                result.AddAttribute(HtmlTextWriterAttribute.Title, "Eliminar");
                result.AddAttribute(HtmlTextWriterAttribute.Id, "chk_" + Id);
                result.RenderBeginTag(HtmlTextWriterTag.Input);
                result.RenderEndTag();
                result.AddAttribute("class", "lbl_conf");
                result.AddAttribute(HtmlTextWriterAttribute.For, "chk_" + Id);
                result.RenderBeginTag(HtmlTextWriterTag.Label);
                result.Write("Eliminar");
                result.RenderEndTag();
            }

            result.RenderEndTag(); //End Td                
            result.RenderEndTag();  //End Tr

            result.RenderEndTag(); // End Table

            result.RenderEndTag(); // End Li

            //result.Close();
            //result.Dispose();

            return result.InnerWriter.ToString();
        }

        public static string Tile(string Etiqueta, string Enlace, string Imagen, string Id, string color, string tamano, string nombre)
        {
            StringWriter stringWriter = new StringWriter();
            HtmlTextWriter result = new HtmlTextWriter(stringWriter);

            result.AddAttribute(HtmlTextWriterAttribute.Class, "recuadros ui-corner-all");
            result.AddAttribute(HtmlTextWriterAttribute.Id, "li-" + Id);
            result.RenderBeginTag(HtmlTextWriterTag.Li); //Begin Li

            result.AddAttribute(HtmlTextWriterAttribute.Style, "width:100%; height:110px; color:white");
            result.RenderBeginTag(HtmlTextWriterTag.Table); //Begin Table

            result.RenderBeginTag(HtmlTextWriterTag.Tr); //Begin Tr
            result.RenderBeginTag(HtmlTextWriterTag.Td); //Begin Td
            result.Write("Etiqueta:");
            result.RenderEndTag(); //End Td
            result.RenderBeginTag(HtmlTextWriterTag.Td); //Begin Td
            result.Write(Etiqueta);
            result.RenderEndTag(); //End Td                
            result.RenderEndTag();  //End Tr

            result.RenderBeginTag(HtmlTextWriterTag.Tr); //Begin Tr
            result.RenderBeginTag(HtmlTextWriterTag.Td); //Begin Td
            result.Write("Imagen:");
            result.RenderEndTag(); //End Td
            result.RenderBeginTag(HtmlTextWriterTag.Td); //Begin Td
            result.AddAttribute(HtmlTextWriterAttribute.Style, "width:71px; height:50px;");
            result.AddAttribute(HtmlTextWriterAttribute.Src, "../Content/RECU_IMG/" + Imagen);
            result.RenderBeginTag(HtmlTextWriterTag.Img); //Begin Img
            result.RenderEndTag(); //End Img
            result.RenderEndTag(); //End Td
            result.RenderEndTag();  //End Tr

            result.RenderBeginTag(HtmlTextWriterTag.Tr); //Begin Tr
            result.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
            result.AddAttribute(HtmlTextWriterAttribute.Align, "center");
            result.RenderBeginTag(HtmlTextWriterTag.Td); //Begin Td 

            if (MySession.Current.Acciones.Contains("M"))
            {
                result.AddAttribute("data-id", Id);
                result.AddAttribute("data-nombre", nombre);
                result.AddAttribute("data-etiqueta", Etiqueta);
                result.AddAttribute("data-imagen", Imagen);
                result.AddAttribute("data-color", color);
                result.AddAttribute("data-tamano", tamano);
                result.AddAttribute(HtmlTextWriterAttribute.Class, "btn_editar");
                result.AddAttribute(HtmlTextWriterAttribute.Type, "radio");
                result.AddAttribute(HtmlTextWriterAttribute.Title, "Editar");
                result.AddAttribute(HtmlTextWriterAttribute.Name, "Group");
                result.AddAttribute(HtmlTextWriterAttribute.Id, "rdb_" + Id);
                result.RenderBeginTag(HtmlTextWriterTag.Input);
                result.RenderEndTag();
                result.AddAttribute("class", "lbl_conf");
                result.AddAttribute(HtmlTextWriterAttribute.For, "rdb_" + Id);
                result.RenderBeginTag(HtmlTextWriterTag.Label);
                result.Write("Editar");
                result.RenderEndTag();
            }

            if (MySession.Current.Acciones.Contains("E"))
            {
                result.AddAttribute("data-id", Id);
                result.AddAttribute(HtmlTextWriterAttribute.Class, "btn_eliminar");
                result.AddAttribute(HtmlTextWriterAttribute.Type, "checkbox");
                result.AddAttribute(HtmlTextWriterAttribute.Title, "Eliminar");
                result.AddAttribute(HtmlTextWriterAttribute.Id, "chk_" + Id);
                result.RenderBeginTag(HtmlTextWriterTag.Input);
                result.RenderEndTag();
                result.AddAttribute("class", "lbl_conf");
                result.AddAttribute(HtmlTextWriterAttribute.For, "chk_" + Id);
                result.RenderBeginTag(HtmlTextWriterTag.Label);
                result.Write("Eliminar");
                result.RenderEndTag();
            }
                        
            result.RenderEndTag(); //End Td                
            result.RenderEndTag();  //End Tr

            result.RenderEndTag(); // End Table

            result.RenderEndTag(); // End Li

            //result.Close();
            //result.Dispose();

            return result.InnerWriter.ToString();
        }               

        public static string Oficina(string NoOficina, string Nombre, string Telefono, string Ext, string Id)
        {
            StringWriter stringWriter = new StringWriter();
            HtmlTextWriter result = new HtmlTextWriter(stringWriter);

            result.AddAttribute(HtmlTextWriterAttribute.Class, "oficinas ui-corner-all");
            result.AddAttribute(HtmlTextWriterAttribute.Id, "li-" + Id);
            result.RenderBeginTag(HtmlTextWriterTag.Li); //Begin Li

            result.AddAttribute(HtmlTextWriterAttribute.Style, "width:100%; height:110px; color:white");
            result.RenderBeginTag(HtmlTextWriterTag.Table); //Begin Table

            result.RenderBeginTag(HtmlTextWriterTag.Tr); //Begin Tr
            result.RenderBeginTag(HtmlTextWriterTag.Td); //Begin Td
            result.Write("No. Oficina:");
            result.RenderEndTag(); //End Td
            result.RenderBeginTag(HtmlTextWriterTag.Td); //Begin Td
            result.Write(NoOficina);
            result.RenderEndTag(); //End Td                
            result.RenderEndTag();  //End Tr

            result.RenderBeginTag(HtmlTextWriterTag.Tr); //Begin Tr
            result.RenderBeginTag(HtmlTextWriterTag.Td); //Begin Td
            result.Write("Nombre:");
            result.RenderEndTag(); //End Td
            result.RenderBeginTag(HtmlTextWriterTag.Td); //Begin Td
            result.Write(Nombre);
            result.RenderEndTag(); //End Td                
            result.RenderEndTag();  //End Tr

            result.RenderBeginTag(HtmlTextWriterTag.Tr); //Begin Tr
            result.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
            result.AddAttribute(HtmlTextWriterAttribute.Align, "center");
            result.RenderBeginTag(HtmlTextWriterTag.Td); //Begin Td 

            if (MySession.Current.Acciones.Contains("M"))
            {
                result.AddAttribute("data-id", Id);
                result.AddAttribute("data-nombre", Nombre);
                result.AddAttribute("data-nooficina", NoOficina );
                result.AddAttribute("data-telefono", Telefono);
                result.AddAttribute("data-ext", Ext);                
                result.AddAttribute(HtmlTextWriterAttribute.Class, "btn_editarO");
                result.AddAttribute(HtmlTextWriterAttribute.Type, "radio");
                result.AddAttribute(HtmlTextWriterAttribute.Title, "Editar");
                result.AddAttribute(HtmlTextWriterAttribute.Name, "Group");
                result.AddAttribute(HtmlTextWriterAttribute.Id, "rdb_" + Id);
                result.RenderBeginTag(HtmlTextWriterTag.Input);
                result.RenderEndTag();
                result.AddAttribute("class", "lbl_conf");
                result.AddAttribute(HtmlTextWriterAttribute.For, "rdb_" + Id);
                result.RenderBeginTag(HtmlTextWriterTag.Label);
                result.Write("Editar");
                result.RenderEndTag();
            }

            if (MySession.Current.Acciones.Contains("E"))
            {
                result.AddAttribute("data-id", Id);
                result.AddAttribute(HtmlTextWriterAttribute.Class, "btn_eliminar");
                result.AddAttribute(HtmlTextWriterAttribute.Type, "checkbox");
                result.AddAttribute(HtmlTextWriterAttribute.Title, "Eliminar");
                result.AddAttribute(HtmlTextWriterAttribute.Id, "chk_" + Id);
                result.RenderBeginTag(HtmlTextWriterTag.Input);
                result.RenderEndTag();
                result.AddAttribute("class", "lbl_conf");
                result.AddAttribute(HtmlTextWriterAttribute.For, "chk_" + Id);
                result.RenderBeginTag(HtmlTextWriterTag.Label);
                result.Write("Eliminar");
                result.RenderEndTag();
            }

            result.RenderEndTag(); //End Td                
            result.RenderEndTag();  //End Tr

            result.RenderEndTag(); // End Table

            result.RenderEndTag(); // End Li

            //result.Close();
            //result.Dispose();

            return result.InnerWriter.ToString();
        }

        public static string Noti(string nombre, string Imagen, string estado, string Id, string fecha)
        {
            StringWriter stringWriter = new StringWriter();
            HtmlTextWriter result = new HtmlTextWriter(stringWriter);

            result.AddAttribute(HtmlTextWriterAttribute.Class, "noti ui-corner-all");
            result.AddAttribute(HtmlTextWriterAttribute.Id, "li-" + Id);
            result.RenderBeginTag(HtmlTextWriterTag.Li); //Begin Li

            result.AddAttribute(HtmlTextWriterAttribute.Style, "width:100%; height:110px; color:white");
            result.RenderBeginTag(HtmlTextWriterTag.Table); //Begin Table

            result.RenderBeginTag(HtmlTextWriterTag.Tr); //Begin Tr
            result.RenderBeginTag(HtmlTextWriterTag.Td); //Begin Td
            result.Write("Nombre:");
            result.RenderEndTag(); //End Td
            result.RenderBeginTag(HtmlTextWriterTag.Td); //Begin Td
            result.Write(nombre);
            result.RenderEndTag(); //End Td                
            result.RenderEndTag();  //End Tr

            result.RenderBeginTag(HtmlTextWriterTag.Tr); //Begin Tr
            result.RenderBeginTag(HtmlTextWriterTag.Td); //Begin Td
            result.Write("Imagen:");
            result.RenderEndTag(); //End Td
            result.RenderBeginTag(HtmlTextWriterTag.Td); //Begin Td
            result.AddAttribute(HtmlTextWriterAttribute.Style, "width:50px; height:71px;");
            result.AddAttribute(HtmlTextWriterAttribute.Src, "../Content/RRHH_IMG/" + Imagen);
            result.RenderBeginTag(HtmlTextWriterTag.Img); //Begin Img
            result.RenderEndTag(); //End Img
            result.RenderEndTag(); //End Td
            result.RenderEndTag();  //End Tr

            result.RenderBeginTag(HtmlTextWriterTag.Tr); //Begin Tr
            result.RenderBeginTag(HtmlTextWriterTag.Td); //Begin Td
            result.Write("Estado:");
            result.RenderEndTag(); //End Td
            result.RenderBeginTag(HtmlTextWriterTag.Td); //Begin Td
            result.Write(estado == "1" ? "Activo" : "Inactivo");
            result.RenderEndTag(); //End Td                
            result.RenderEndTag();  //End Tr

            result.RenderBeginTag(HtmlTextWriterTag.Tr); //Begin Tr
            result.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
            result.AddAttribute(HtmlTextWriterAttribute.Align, "center");
            result.RenderBeginTag(HtmlTextWriterTag.Td); //Begin Td 

            if (MySession.Current.Acciones.Contains("M"))
            {
                result.AddAttribute("data-id", Id);
                result.AddAttribute("data-nombre", nombre);
                result.AddAttribute("data-imagen", Imagen);
                result.AddAttribute("data-estado", estado);
                result.AddAttribute("data-fecha", fecha);

                result.AddAttribute(HtmlTextWriterAttribute.Class, "btn_editarN");
                result.AddAttribute(HtmlTextWriterAttribute.Type, "radio");
                result.AddAttribute(HtmlTextWriterAttribute.Title, "Editar");
                result.AddAttribute(HtmlTextWriterAttribute.Name, "Group");
                result.AddAttribute(HtmlTextWriterAttribute.Id, "rdb_" + Id);
                result.RenderBeginTag(HtmlTextWriterTag.Input);
                result.RenderEndTag();
                result.AddAttribute("class", "lbl_conf");
                result.AddAttribute(HtmlTextWriterAttribute.For, "rdb_" + Id);
                result.RenderBeginTag(HtmlTextWriterTag.Label);
                result.Write("Editar");
                result.RenderEndTag();
            }

            if (MySession.Current.Acciones.Contains("E"))
            {
                result.AddAttribute("data-id", Id);
                result.AddAttribute(HtmlTextWriterAttribute.Class, "btn_eliminar");
                result.AddAttribute(HtmlTextWriterAttribute.Type, "checkbox");
                result.AddAttribute(HtmlTextWriterAttribute.Title, "Eliminar");
                result.AddAttribute(HtmlTextWriterAttribute.Id, "chk_" + Id);
                result.RenderBeginTag(HtmlTextWriterTag.Input);
                result.RenderEndTag();
                result.AddAttribute("class", "lbl_conf");
                result.AddAttribute(HtmlTextWriterAttribute.For, "chk_" + Id);
                result.RenderBeginTag(HtmlTextWriterTag.Label);
                result.Write("Eliminar");
                result.RenderEndTag();
            }

            result.RenderEndTag(); //End Td                
            result.RenderEndTag();  //End Tr

            result.RenderEndTag(); // End Table

            result.RenderEndTag(); // End Li

            //result.Close();
            //result.Dispose();

            return result.InnerWriter.ToString();
        }  

        public static string Menus(string ment_id, string mend_id, string nombre, string enlace, string padre)
        {
            StringWriter stringWriter = new StringWriter();
            HtmlTextWriter result = new HtmlTextWriter(stringWriter);

            result.AddAttribute(HtmlTextWriterAttribute.Class, "menus ui-corner-all");
            result.AddAttribute(HtmlTextWriterAttribute.Id, "li-"  + mend_id);
            result.RenderBeginTag(HtmlTextWriterTag.Li); //Begin Li

            result.AddAttribute(HtmlTextWriterAttribute.Style, "width:100%; height:110px; color:white");
            result.RenderBeginTag(HtmlTextWriterTag.Table); //Begin Table

            result.RenderBeginTag(HtmlTextWriterTag.Tr); //Begin Tr
            result.RenderBeginTag(HtmlTextWriterTag.Td); //Begin Td
            result.Write("Nombre:");
            result.RenderEndTag(); //End Td
            result.RenderBeginTag(HtmlTextWriterTag.Td); //Begin Td
            result.Write(nombre);
            result.RenderEndTag(); //End Td                
            result.RenderEndTag();  //End Tr

            result.RenderBeginTag(HtmlTextWriterTag.Tr); //Begin Tr
            result.RenderBeginTag(HtmlTextWriterTag.Td); //Begin Td
            result.Write("Enlace:");
            result.RenderEndTag(); //End Td
            result.RenderBeginTag(HtmlTextWriterTag.Td); //Begin Td
            result.Write(enlace);
            result.RenderEndTag(); //End Td                
            result.RenderEndTag();  //End Tr

            result.RenderBeginTag(HtmlTextWriterTag.Tr); //Begin Tr
            result.RenderBeginTag(HtmlTextWriterTag.Td); //Begin Td
            result.Write("Padre:");
            result.RenderEndTag(); //End Td
            result.RenderBeginTag(HtmlTextWriterTag.Td); //Begin Td
            result.Write(padre);
            result.RenderEndTag(); //End Td                
            result.RenderEndTag();  //End Tr

            result.RenderBeginTag(HtmlTextWriterTag.Tr); //Begin Tr
            result.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
            result.AddAttribute(HtmlTextWriterAttribute.Align, "center");
            result.RenderBeginTag(HtmlTextWriterTag.Td); //Begin Td 

            if (MySession.Current.Acciones.Contains("M")) //Para Modificar
            {
                result.AddAttribute("data-id", mend_id);
                result.AddAttribute("data-tid", ment_id);
                result.AddAttribute("data-nombre", nombre);
                result.AddAttribute("data-enlace", enlace);
                result.AddAttribute("data-padre", padre);
                result.AddAttribute(HtmlTextWriterAttribute.Class, "btn_editarm");
                result.AddAttribute(HtmlTextWriterAttribute.Type, "radio");
                result.AddAttribute(HtmlTextWriterAttribute.Title, "Editar");
                result.AddAttribute(HtmlTextWriterAttribute.Name, "Group");
                result.AddAttribute(HtmlTextWriterAttribute.Id, "rdb_" + mend_id);
                result.RenderBeginTag(HtmlTextWriterTag.Input);
                result.RenderEndTag();
                result.AddAttribute("class", "lbl_conf");
                result.AddAttribute(HtmlTextWriterAttribute.For, "rdb_" + mend_id);
                result.RenderBeginTag(HtmlTextWriterTag.Label);
                result.Write("Editar");
                result.RenderEndTag();
            }

            if (MySession.Current.Acciones.Contains("E")) //Para eliminar
            {
                result.AddAttribute("data-id", mend_id);
                result.AddAttribute(HtmlTextWriterAttribute.Class, "btn_eliminar");
                result.AddAttribute(HtmlTextWriterAttribute.Type, "checkbox");
                result.AddAttribute(HtmlTextWriterAttribute.Title, "Eliminar");
                result.AddAttribute(HtmlTextWriterAttribute.Id, "chk_" + mend_id);
                result.RenderBeginTag(HtmlTextWriterTag.Input);
                result.RenderEndTag();
                result.AddAttribute("class", "lbl_conf");
                result.AddAttribute(HtmlTextWriterAttribute.For, "chk_" + mend_id);
                result.RenderBeginTag(HtmlTextWriterTag.Label);
                result.Write("Eliminar");
                result.RenderEndTag();
            }


            result.RenderEndTag(); //End Td                
            result.RenderEndTag();  //End Tr

            result.RenderEndTag(); // End Table

            result.RenderEndTag(); // End Li

            //result.Close();
            //result.Dispose();

            return result.InnerWriter.ToString();
        }

        /// <summary>
        /// Funcion que obtiene los permisos a los botones de cada forma
        /// </summary>
        /// <param name="SegId">Id de la Asignación del Role y Modulo</param>
        public static void Acciones(string SegId)
        {
            DataTable dt = new DataTable();
            string Acc = "";
            string Segs = "";

            var a = SegId.Split('-');

            foreach (var item in a)
            {
                if(item != "")
                {
                    Segs = Segs + "'" + item + "',";
                }
            }                       

            dt = RunQuery(Resource_Combex.VAL_ACCIONESXUSUARIOS.Replace("_var1_", Segs.Substring(0,Segs.Length -1)), "Val").Tables["Val"];
                        
            foreach (DataRow item in dt.Rows)
            {
                Acc = Acc + item["SEG_BOTON"].ToString() + ", ";
            }

            MySession.Current.Acciones = Acc;
           
        }        

        /// <summary>
        /// Funcion que genera un GUID unico para cada archivo 
        /// </summary>
        /// <param name="ext">Extenxion de los archivos</param>
        /// <returns></returns>
        public static string GenUFN(string ext)
        {
            return DateTime.Now.Ticks.ToString("x") + ext;
        }

        /// <summary>
        /// Esta Funcion obtiene el valor de los parametros de DB
        /// </summary>
        /// <param name="CIA">Asociación</param>
        /// <param name="PARAMETRO">Nombre del paramentro</param>
        /// <returns></returns>
        public static string Get_Param(string CIA, string PARAMETRO)
        {
            string res = "";
            var dt = OracleFunc.RunQuery("select cb1.get_param('" + CIA + "','" + PARAMETRO + "') Param from dual", "Param").Tables["Param"];
            if (dt.Rows.Count > 0)
            {
                res = dt.Rows[0][0].ToString();
            }

            return res;
        }

        public static string Get_CobroSegRxGt(string CIA, string TIPOGUIA)
        {
            string res = "";
            var dt = OracleFunc.RunQuery("select cb1.get_cobrosegrxgt('" + CIA + "','" + TIPOGUIA + "') Param from dual", "Param").Tables["Param"];
            if (dt.Rows.Count > 0)
            {
                res = dt.Rows[0][0].ToString();
            }
            return res;
        }

        /// <summary>
        /// Obtiene los descuentos para la guia
        /// </summary>
        /// <param name="HileraGuia">recibe el GuiaCorr + Guia_Anio + TipoGuia_Cod</param>
        /// <returns></returns>
        public static double Get_Descuento(string HileraGuia)
        {
            double res = 0;
            var dt = OracleFunc.RunQuery("SELECT NVL(DES_MONTO,0) DES_MONTO FROM CB1.CBX_DESCU WHERE GUIA_CORR||GUIA_ANIO||TIPOGUIA_COD = '" + HileraGuia + "' AND DES_ESTADO = 'P'", "Descuento").Tables["Descuento"];
            if (dt.Rows.Count > 0)
            {
                res = double.Parse(dt.Rows[0]["DES_MONTO"].ToString());
            }
            return res;
        }

        /// <summary>
        /// Obtiene el numero de operacion
        /// </summary>
        /// <returns></returns>
        public static double Get_FacOper()
        {
            double res = 0;
            var dt = OracleFunc.RunQuery("SELECT cbx_fac_oper.nextval Fac_Oper FROM DUAL", "Fac_Oper").Tables["Fac_Oper"];
            if (dt.Rows.Count > 0)
            {
                res = double.Parse(dt.Rows[0]["FAC_OPER"].ToString());
            }
            return res;
        }

        /// <summary>
        /// Carga y redimensiona las imagenes
        /// </summary>
        /// <param name="file">Archivos de imagen cargado en el navegador</param>
        /// <param name="fname">Ruta y nombre del archivo</param>
        /// <param name="Width">Ancho de la imagen</param>
        /// <param name="Height">Alto de la imagen</param>
        /// <param name="resizemode">Tipo de redimension 0 = Original size 1 = Force size, preserving aspect ratio 2 = Keep original size, enforce max dimentions </param>
        /// <returns></returns>
        public static Boolean UploadImageNoThumb(HttpPostedFile file, string fname, int Width, int Height, int resizemode = 0)
        {
            int newOWidth;
            int newOHeight; //new width/height for the original
            //int l2;         //temp variable used when calculating new size
            bool resize = false;

            System.Drawing.Image originalimg; //used to hold the original image

            originalimg = System.Drawing.Image.FromStream(file.InputStream);
            newOWidth = originalimg.Width;
            newOHeight = originalimg.Height;
            switch (resizemode)
            {
                case 1: //Force specific size preserving aspect ratio
                    resize = true;
                    break;
                case 2: //Keep original size, enforce max dimentions
                    if (originalimg.Width > Width || originalimg.Height > Height)
                    {
                        resize = true;
                    }
                    else
                    {
                        resize = false;
                    }
                    break;
                case 0: //Mantain original size
                default:
                    resize = false;
                    break;
            }
            if (resize)
            {
                //Mejoras en el cambio de tamaño de las imagenes
                newOWidth = Width;
                newOHeight = Height;

                //work out the width/height for the thumbnail. Preserve aspect ratio and honour max width/height
                //***RESIZE START***
                //if ((Convert.ToDouble(originalimg.Width) / Convert.ToDouble(Width)) > (Convert.ToDouble(originalimg.Height) / Convert.ToDouble(Height)))
                //{
                //    //Original
                //    l2 = originalimg.Width;
                //    newOWidth = Width;
                //    newOHeight = Convert.ToInt32(Convert.ToDouble(originalimg.Height) * (Convert.ToDouble(Width) / Convert.ToDouble(l2)));
                //    if (newOHeight > Height)
                //    {
                //        newOWidth = Convert.ToInt32(Convert.ToDouble(newOWidth) * (Convert.ToDouble(Height) / Convert.ToDouble(newOHeight)));
                //        newOHeight = Height;
                //    }
                //}
                //else
                //{
                //    //Original
                //    l2 = originalimg.Height;
                //    newOHeight = Height;
                //    newOWidth = Convert.ToInt32(Convert.ToDouble(originalimg.Width) * (Convert.ToDouble(Height) / Convert.ToDouble(l2)));
                //    if (newOWidth > Width)
                //    {
                //        newOHeight = Convert.ToInt32(Convert.ToDouble(newOHeight) * (Convert.ToDouble(Width) / Convert.ToDouble(newOWidth)));
                //        newOWidth = Width;
                //    }
                //}
            }
            Bitmap neworig = new Bitmap(newOWidth, newOHeight);
            //Create a graphics object
            Graphics gr_destO = Graphics.FromImage(neworig);
            //Re-draw the image to the specified height and width
            gr_destO.DrawImage(originalimg, 0, 0, neworig.Width, neworig.Height);
            //***RESIZE END***

            neworig.Save(fname, originalimg.RawFormat);
            return true;
        }


        /// <summary>
        /// Convertir archivos png a jpeg
        /// </summary>
        /// <param name="Query">Nombre del procedimiento almacenado</param>
        /// <param name="Parameters">Parametros que recibe el procedimiento almacenado</param>
        /// <returns></returns>

        public static Image ConvertPNGtoJPEG(string filePath, ImageFormat imageFormat) {
            Image imageOriginal = Image.FromFile(filePath);
            Image imageConverted = null;

            using (MemoryStream ms = new MemoryStream())
            {
                imageOriginal.Save(ms, imageFormat);
                ms.Position = 0; // rewind stream
                imageConverted = Image.FromStream(ms);
   
            }
            return imageConverted;
        }

        public static string DataTableToJson(DataTable table)
        {
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(table);
            return JSONString;
        }

        public static string DataTableToCSV(DataTable table) {
          // string v_CSV = string.Empty;

            StringBuilder builder = new StringBuilder();

            for(int i= 0; i< table.Columns.Count ; i++){
                //El registro es la cabera con los nombres de las columnas.
                builder.Append(table.Columns[i].ColumnName );
                if (i == table.Columns.Count -1)
                {
                    builder.Append("|");
                }
                else {
                    builder.Append(";");
                }
            }

            for (int i = 0; i < table.Rows.Count; i++)
            {
                for (int j = 0; j < table.Rows[i].Table.Columns.Count; j++)
                {
                    //Guardamos cada fila separada por coma.
                    builder.Append(table.Rows[i][j].ToString());

                    if (j == table.Columns.Count -1)
                    {
                        builder.Append("|");
                    }
                    else {
                        builder.Append(";");
                    }
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Funcion para generar claves aleatorias
        /// </summary>
        /// <param name="LongPassMin">Minimo de caracteres de la clave</param>
        /// <param name="LongPassMax">Maximo de caracteres de la clave</param>
        /// <returns>Clave</returns>
        public static string GenerarPass(int LongPassMin, int LongPassMax)
        {

            char[] ValueAfanumeric = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'Q', 'W', 'E', 'R', 'T', 'Y', 'U', 'I', 'O', 'P', 'A', 'S', 'D', 'F', 'G', 'H', 'J', 'K', 'L', 'Z', 'X', 'C', 'V', 'B', 'N', 'M', '!', '#', '$', '%', '&', '?', '¿' };

            Random ram = new Random();
            int LogitudPass = ram.Next(LongPassMin, LongPassMax);
            string Password = String.Empty;

            for (int i = 0; i < LogitudPass; i++)
            {
                int rm = ram.Next(0, 2);

                if (rm == 0)
                {
                    Password += ram.Next(0, 10);
                }
                else
                {
                    Password += ValueAfanumeric[ram.Next(0, 42)];
                }
            }

            return Password;
        }

        /// <summary>
        /// Funcion para cambiar el tamaño de las imagenes
        /// </summary>
        /// <param name="imagen">Objeto imagen con la imagen que se desea redimensionar</param>
        /// <param name="ancho">Nuevo ancho de la imagen</param>
        /// <param name="alto">Nuevo alto de la imagen</param>
        /// <returns>La imagen con nuevas dimensiones</returns>
        public static Image Redimensionar(Image imagen, int ancho, int alto)
        {

            if (imagen.Width > imagen.Height)
            {
                alto = imagen.Height * ancho / imagen.Width;
            }
            else
            {
                ancho = imagen.Width * alto / imagen.Height;
            }

            Image imagenPhoto = imagen;


            Bitmap bmPhoto = new Bitmap(ancho, alto, PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(72, 72);
            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.SmoothingMode = SmoothingMode.AntiAlias;
            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
            grPhoto.PixelOffsetMode = PixelOffsetMode.HighQuality;
            grPhoto.DrawImage(imagenPhoto, new Rectangle(0, 0, ancho, alto), 0, 0, imagen.Width, imagen.Height, GraphicsUnit.Pixel);

            MemoryStream mm = new MemoryStream();
            bmPhoto.Save(mm, System.Drawing.Imaging.ImageFormat.Jpeg);
            imagen.Dispose();
            imagenPhoto.Dispose();
            bmPhoto.Dispose();
            grPhoto.Dispose();
            return byteArrayToImage(mm.GetBuffer());
        }

        public static Image byteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            System.Drawing.Image returnimage = default(System.Drawing.Image);
            returnimage = System.Drawing.Image.FromStream(ms);

            return returnimage;
        }


    }

    //CLASE PARA EL MENU RECURSIVO
    public class Menud
    {
        public Menud()
        {            
        }

        public List<decimal> Seg_Gene_Id { get; set; }
        //public decimal Seg_Gene_Id { get; set; }
        public short Mend_Id { get; set; }
        public string Mend_Nombre { get; set; }
        public string Mend_Enlace { get; set; }
        public Nullable<short> Mend_Padre { get; set; }
        public short Mend_Orden { get; set; }
                
    }   

    //CLASE PARA LAS VALIDACIONES
    public class Val
    {
        public Val()
        {
        }

        public short Val_Id { get; set; }
        public string Val_Desc { get; set; }
        public Nullable<short> Val_Parent { get; set; }
        public string Val_Valida { get; set; }
        public string GafVal_Resp { get; set; }
        public string GafVal_Motivo { get; set; }
        public string GafVal_Vence { get; set; }
    }
 
    /// <summary>
    /// Clase para creo el un registro de los errores
    /// </summary>
    public class CreateLogFiles
    {
        private CreateLogFiles()
        {
        }
        
        /// <summary>
        /// Envia los mensajes de error a archivo para verificarlos posteriormente
        /// </summary>
        /// <param name="sPathName">Ruta del archivo</param>
        /// <param name="sErrMsg">Mensaje de la Excepción</param>
        public static void ErrorLog(string sPathName, string sErrMsg)
        {
            string sLogFormat;
            string sErrorTime;

             //sLogFormat used to create log files format :
             // dd/mm/yyyy hh:mm:ss AM/PM ==> Log Message
             sLogFormat = DateTime.Now.ToShortDateString().ToString()+" "+DateTime.Now.ToLongTimeString().ToString()+" ==> ";
            
            //this variable used to create log filename format "
            //for example filename : ErrorLogYYYYMMDD
            string sYear    = DateTime.Now.Year.ToString();
            string sMonth    = DateTime.Now.Month.ToString();
            string sDay    = DateTime.Now.Day.ToString();
            sErrorTime = sYear+sMonth+sDay;

            StreamWriter sw = new StreamWriter(sPathName+sErrorTime,true);
            sw.WriteLine(sLogFormat + sErrMsg);
            sw.Flush();
            sw.Close();
        }

        /// <summary>
        /// Crea y escribe los archivos de error generados en el proceso de guatefacturas
        /// </summary>
        /// <param name="sPathName">Ruta del archivo</param>
        /// <param name="sErrMsg">Mensaje de error</param>
        public static void ErrorLog_GuateFac(string sPathName, string sErrMsg)
        {
            StreamWriter sw = new StreamWriter(sPathName, true);
            sw.WriteLine(sErrMsg);
            sw.Flush();
            sw.Close();
        }
    }

    /// <summary>
    /// Esta clase ayuda a evitar SQL Injection
    /// </summary>
    public static class MyExtensions
    {
        public static string Sanitize(this string stringValue)
        {
            if (null == stringValue)
                return stringValue;
            return stringValue
                        .RegexReplace("-{2,}", "-")                 // transforms multiple --- in - use to comment in sql scripts
                        .RegexReplace(@"[;]+", "")                 // removes ' and ;     
                        .Replace(@"/*", "").Replace(@"*/", "");      // removes /* used also to comment in sql scripts
            //.RegexReplace(@"(;|\s)(exec|execute|select|insert|update|delete|create|alter|drop|rename|truncate|backup|restore)\s", "", RegexOptions.IgnoreCase);
        }

        private static string RegexReplace(this string stringValue, string matchPattern, string toReplaceWith)
        {
            return Regex.Replace(stringValue, matchPattern, toReplaceWith);
        }

        private static string RegexReplace(this string stringValue, string matchPattern, string toReplaceWith, RegexOptions regexOptions)
        {
            return Regex.Replace(stringValue, matchPattern, toReplaceWith, regexOptions);
        }
    }

    public class Lovs
    {
        public string value { get; set; }
        public string label { get; set; }

        public string cli_id { get; set; }

        public string pais_origen { get; set; }
        public string epais_origen { get; set; }
        public string pais_destino { get; set; }
        public string epais_destino { get; set; }
        public string vuelo_cargaPasajero { get; set; }

        public int man_corr { get; set;}
        public int man_anio { get; set; }
        public string tipoman_cod { get; set; }

        public string prefijo { get; set; }

    }


    public class select2Filter
    {
        public string term { get; set; }
        public string _type { get; set; }
    }
}
