<?php
session_start();
error_reporting(E_ERROR | E_PARSE);
//if (!$_SESSION["usuario_id"] != null){
    include '../../../php/core_functions.php';
	//header("Access-Control-Allow-Origin: http://127.0.0.1:5500");
	header("Access-Control-Allow-Origin: *");
	header("Access-Control-Allow-Headers: *");
	header('Content-Type: application/json; charset=utf-8');
	//echo "sss".$_SERVER['HTTP_ORIGIN'];
    try {
      if (isset($_GET["FUNC"])) {
        $v_Func = $_GET["FUNC"];
		if($v_Func == "getMenuDelDia"){
			echo getMenuDelDia($_GET["fechaActual"],$_GET["horaActual"]);
		}else if($v_Func == "getMarcajes"){
			echo getMarcajes();
		}else if($v_Func == "duca"){
			echo duca();
		}else if($v_Func == "procesos"){
			echo procesos($_GET["proceso"]);
		}else if($v_Func == "crearPedido"){
			echo crearPedido();
		}else if($v_Func == "crearPedidoa"){

			$message = array(
				"message" => "Se ha guardado el pedido."
			);

			echo json_encode($message,JSON_NUMERIC_CHECK);;
		}
		else{
			echo "opcion no existe";
		}
		
      }
    } catch (Exception $e) {
        echo $e->getMessage();
    }
/*}
else{
    header("HTTP/1.0 401 Not Authorized");
}*/

function getMenuDelDia($fechaActual,$horaActual){
	$conexion = _connectDB();
	try {
		$strConsulta = "SELECT
		MDET_DIA, 
		CAFE_DESC,TRIM(to_char(MDET_DIA, 'DAY', 'NLS_DATE_LANGUAGE=SPANISH'))||' '||to_char(MDET_DIA,'DD/MM/YYYY') DIA
		, to_char(MDET_DIA,'DD/MM/YYYY HH24:MI:SS') FECHA, MDET_PLATO
		, A.MENU_ID
		,CAFE_HORA_DE
		,CAFE_HORA_A
		,MDET_ID
		,'$fechaActual'
		FROM SAB.SAB_RRHH_CAFE_MENU A 
		INNER JOIN 
		SAB.SAB_RRHH_CAFE B
		ON A.CAFE_ID=B.CAFE_ID
		INNER JOIN SAB.SAB_RRHH_CAFE_MDET C
		ON A.MENU_ID=C.MENU_ID
		WHERE 
		--MENU_ESTADO = 'A' AND 
		to_char(MDET_DIA,'YYYY-MM-DD')='$fechaActual'
		AND $horaActual>=CAFE_HORA_DE
		AND $horaActual<=CAFE_HORA_A";
		$arrConsulta = _query($strConsulta);

		$consulta = oci_parse($conexion, $strConsulta);
		$r = oci_execute($consulta);
		
		if (!$r){
			$e = oci_error($consulta);
			throw new Exception($e['message']);
		}
	
		if(count($arrConsulta[0])==0){
			$alerta = array(
				"warning" => "No existe menú para el horario actual."
			);
			return json_encode($alerta,JSON_NUMERIC_CHECK);
		};
		//if($strConsulta[0])

		//QUITAR NUMEROS DE KEY
		foreach ($arrConsulta as $index => $arrayIndex) {
			foreach ($arrConsulta[$index] as $key3 => $arrayItem3) {
				if(is_numeric($key3)){
					unset($arrConsulta[$index][$key3]);
				}
			}
		}

		$arrConsulta = getDetalleMenu($arrConsulta);
		$arrCorresponde = getHorarioAsignado($fechaActual,$_GET["emp"]);
		$arrPedidoEmpleado = getPedidoDelDiaEmpleado($fechaActual,$_GET["emp"]);

		if(!isset($arrCorresponde["START_TIME"])){
			$alerta = array(
				"warning" => "No cuenta con horario registrado para el dia de hoy."
			);
			return json_encode($alerta,JSON_NUMERIC_CHECK);
		}else if(!($horaActual>=$arrCorresponde["START_TIME"] && $horaActual<=$arrCorresponde["END_TIME"])){
			$alerta = array(
				"warning" => "Su horario establecio no corresponde al menu actual."
			);
			return json_encode($alerta,JSON_NUMERIC_CHECK);
		}else if(isset($arrPedidoEmpleado[0]["PEDI_ID"])){
			$alerta = array(
				"warning" => "Ya realizó un consumo el día de hoy."
			);
			return json_encode($alerta,JSON_NUMERIC_CHECK);
		}
		
		$data = array(
			"data" => $arrConsulta,
			//"horario" =>$arrCorresponde,
		);

		$data = array_change_key_case_recursive($data);
		return json_encode($data,JSON_NUMERIC_CHECK);
	}catch(Exception $e) {
		$message = array(
			"error" => $e->getMessage()
		);

		http_response_code(500);
		return json_encode($message);
	}
}


function procesos($proceso){
	$conexion = _connectDB();
	try {
		if($proceso==1){
			$strConsulta = "SELECT GAFD_COD,GAFD_ANIO,gafd_docnum FROM SAB.SAB_SACL_GAFH
			WHERE GAFD_ANIO=2023 ORDER BY GAFD_COD DESC";
			$arrConsulta = _query($strConsulta);

			$consulta = oci_parse($conexion, $strConsulta);
			$r = oci_execute($consulta);
			
			if (!$r){
				$e = oci_error($consulta);
				throw new Exception($e['message']);
			}
			foreach ($arrConsulta as $key => $arrayItem) {
				$GAFD_COD =$arrayItem['GAFD_COD'];
				$GAFD_ANIO =$arrayItem['GAFD_ANIO'];
				$GAFD_DOCNUM =$arrayItem['GAFD_DOCNUM'];
				$strSub = "INSERT INTO SAB.SAB_SACL_ARCH_GAFH(GAFD_COD,GAFD_ANIO,GAFD_DOCNUM,ID_ARCHIVO)
				SELECT $GAFD_COD,$GAFD_ANIO,'$GAFD_DOCNUM',ID_ARCHIVO 
				FROM SAB.SAB_GENE_ARCHIVO WHERE ID_ARCHIVO IN (13,14,4,5,9,10,11)  
				AND ID_ARCHIVO NOT IN (SELECT ID_ARCHIVO FROM SAB.SAB_SACL_ARCH_GAFH WHERE GAFD_COD=$GAFD_COD AND GAFD_ANIO=$GAFD_ANIO AND gafd_docnum='$GAFD_DOCNUM')";
				$arrSubConsulta = _query($strSub);

				$consulta = oci_parse($conexion, $strSub);
				$r = oci_execute($consulta);
				
				if (!$r){
					$e = oci_error($consulta);
					throw new Exception("Error en insertar documento: ".$e['message']);
				};
			}
			$data = array(
				"data" => "Proceso realizado",
			);

			return json_encode($data,JSON_NUMERIC_CHECK);
		}
	}catch(Exception $e) {
		$message = array(
			"error" => $e->getMessage()
		);

		http_response_code(500);
		return json_encode($message);
	}
}

function duca(){
	ini_set('display_errors', 1);
    ini_set('display_startup_errors', 1);
    error_reporting(E_ALL);

    $pDocTransporte = 8676187;
    $pManifiesto="06D22006628";

    $url = "https://farm3.sat.gob.gt/declaracion-mercancias-transp/rest/privado/guiaTransporte/consultarDeclaracionPorDocTransporte?pDocTransporte=$pDocTransporte&pManifiesto=$pManifiesto";
    $curl = curl_init();
    $ch = curl_init($url);
    curl_setopt($ch, CURLOPT_TIMEOUT, 180);
    curl_setopt($ch, CURLOPT_CONNECTTIMEOUT, 180);
    curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
	curl_setopt($ch, CURLOPT_ENCODING, "UTF8");
	
    curl_setopt($ch, CURLOPT_HTTPHEADER, array(
        'Authorization: basic NTI0NDkwOTI6MjAxOUNvbWJleGlt'
    ));

   

    $data = curl_exec($ch);

	
    $objetoJSON = json_decode($data,true);
	
	$strResultado  = "";
	$strResultado .= "{";
	$strResultado .= "\"tipo\":\"" . $objetoJSON["tipo"] ."\",";
	$strResultado .= "\"codigo\":" . $objetoJSON["codigo"] .",";
	if ( $objetoJSON["tipo"] == "EXITO" )
	{
		$strResultado .= "\"mensaje\":\"" . $objetoJSON["mensaje"] ."\",";
		$strResultado .= "\"valor\":\"";
		$valores = $objetoJSON["valor"];
		//return  json_encode($valores);
		for ( $contador = 0; $contador < count($objetoJSON["valor"]); $contador++)
		{
			//return  json_encode(explode(",",""));
			$arreglo = explode(",","
					noManifiestoMaster: 03A22011450,
					noDocumentoTransporteHijo: 8676187,
					numeroDeclaracion: GTGUAEA-22-183529-0001-1,
					noManifiestoHijo: 06D22006628,
					noDocumentoTransporteMaster: 40604458392,
					noCorrelativo: 318-2720687
				}
			");
			$data = str_replace("}","",$arreglo[1]);
			$data = str_replace(" Declaración=","",$data);
			$strResultado .= "" . trim($data) . "|";
		}

		// for ( $contador = 0; $contador < count($objetoJSON["valor"]); $contador++)
		// {
		// 	$arreglo = explode(",",$valores[$contador]);
		// 	$data = str_replace("}","",$arreglo[1]);
		// 	$data = str_replace(" Declaración=","",$data);
		// 	$strResultado .= "" . trim($data) . "|";
		// }
		$strResultado = substr_replace($strResultado ,"", -1);
		$strResultado .= "\",";
		$strResultado .= "\"fechaHora\":\"" . $objetoJSON["fechaHora"] ."\"";
	}
	else
	{
		$strResultado .= "\"mensaje\":\"" . $objetoJSON["mensaje"] ."\"";
	}
	$strResultado .= "}";
	return $strResultado;
}

function getMarcajes(){
	$conexion = _connectDB();
	try {
		$strConsulta = "SELECT 
		USER_ID
		, TIME_LOG_ID
        ,TO_CHAR(CREATE_DATE,'DD/MM/YYYY HH24:MI:SS') CREATE_DATE
		,TO_CHAR(CREATE_DATE,'YYYY-MM-DD') FECHA_MARCAJE
        --,'http://10.238.20.204:8089/sideima/'||FOTO FOTO
        ,B.NOMBRE
        ,APELLIDO
        ,B.CODEMP
        ,DPI
	  FROM 
	  dbo.USER_TIME_LOG@RELOJSQL A, cb1.v_empleados_combex B
	  WHERE  
		DEVICE_ID IN (62,66)
		AND AUTHORIZE_REASON_CODE = 'Access'
		--AND TRUNC(CREATE_DATE)>=TRUNC(SYSDATE-1)
		AND TRUNC(CREATE_DATE)=TRUNC(SYSDATE)
		AND regexp_replace(regexp_replace(CODEMP, '[^0-9]', ''),'0','',1,1)=USER_ID
	  ORDER BY TIME_LOG_ID DESC";
		$arrConsulta = _query($strConsulta);

		//QUITAR NUMEROS DE KEY
		foreach ($arrConsulta as $index => $arrayIndex) {
			foreach ($arrConsulta[$index] as $key3 => $arrayItem3) {
				if(is_numeric($key3)){
					unset($arrConsulta[$index][$key3]);
				}
			}
		}
		$data = array(
			"data" => ubicarParametros($arrConsulta)
		);
		
		return json_encode($data,JSON_NUMERIC_CHECK);
	}catch(Exception $e) {
		$message = array(
			"error" => $e->getMessage()
		);

		http_response_code(500);
		return json_encode($message);
	}
}

function ubicarParametros($arrConsulta){
	
	$conexion = _connectDB();
	
	foreach ($arrConsulta as $key => $arrayItem) {
		//return $arrayItem;
		$DPI =$arrayItem['DPI'];
		$strSub = "SELECT  'http://10.238.20.204:8089/sideima/'||FOTO FOTO FROM SAB.V_GAFETES_COMBEX D 
		WHERE D.GAFD_ANIO = (SELECT  MAX(GAFD_ANIO) FROM SAB.V_GAFETES_COMBEX DD WHERE SUBSTR(D.GAFD_GAFETE,3,5)=SUBSTR(DD.GAFD_GAFETE,3,5) )
		AND d.gafd_docnum='$DPI'";
		$arrSubConsulta = _query($strSub);
		$consulta = oci_parse($conexion, $strSub);
		$r = oci_execute($consulta);
		
		if (!$r){
			$e = oci_error($consulta);
			throw new Exception($e['message']);
		};		
			
		$fechaActual =$arrayItem['FECHA_MARCAJE'];
		$codemp =$arrayItem['CODEMP'];
		$arrConsulta[$key]["foto"]= $arrSubConsulta[0]["FOTO"];


		$arrPedidosRealizados = getPedidoDelDiaEmpleado($fechaActual,$codemp);
		$arrConsulta[$key]["cantidad_pedidos"]= count($arrPedidosRealizados);
		$arrConsulta[$key]["pedidos"]= $arrPedidosRealizados;
	}

	return array_change_key_case_recursive($arrConsulta);
	
}

function getDetalleMenu($arrConsulta){
	
	$conexion = _connectDB();
	
	foreach ($arrConsulta as $key => $arrayItem) {
		//return $arrayItem;
		$MDET_ID =$arrayItem['MDET_ID'];
		$strSub = "SELECT
					mcom_id,
					mdet_id,
					mcom_no,
					mcom_complemento,
					mcom_tipo
					,'true' pedir
					FROM
					sab.sab_rrhh_cafe_mcom where MDET_ID=$MDET_ID AND MCOM_TIPO='G'";
		$arrSubConsulta = _query($strSub);

		$consulta = oci_parse($conexion, $strSub);
		$r = oci_execute($consulta);
		
		if (!$r){
			$e = oci_error($consulta);
			throw new Exception("Error en detalles de menu: ".$e['message']);
		};
		foreach ($arrSubConsulta as $index => $arrayIndex) {
			foreach ($arrSubConsulta[$index] as $key3 => $arrayItem3) {
				if(is_numeric($key3)){
					unset($arrSubConsulta[$index][$key3]);
				}
			}
		}
		
		$arrConsulta[$key]["complementos"]= $arrSubConsulta;

		$strSub = "SELECT
					mcom_id,
					mdet_id,
					mcom_no,
					mcom_complemento,
					mcom_tipo
					,'true' pedir
					FROM
					sab.sab_rrhh_cafe_mcom where MDET_ID=$MDET_ID AND MCOM_TIPO='P'";
		$arrSubConsulta = _query($strSub);

		$consulta = oci_parse($conexion, $strSub);
		$r = oci_execute($consulta);
		
		if (!$r){
			$e = oci_error($consulta);
			throw new Exception("Error en detalles de postre: ".$e['message']);
		};
		foreach ($arrSubConsulta as $index => $arrayIndex) {
			foreach ($arrSubConsulta[$index] as $key3 => $arrayItem3) {
				if(is_numeric($key3)){
					unset($arrSubConsulta[$index][$key3]);
				}
			}
		}
		
		$arrConsulta[$key]["postres"]= $arrSubConsulta;
	}

	return $arrConsulta;
}

function getPedidoDelDiaEmpleado($fechaActual,$codemp){
	$conexion = _connectDB();
	
	$strSub = "SELECT * FROM SAB.SAB_RRHH_CAFE_PEDI
	WHERE PEDI_CODNAV='$codemp'
	AND to_char(PEDI_FECHA,'YYYY-MM-DD')='$fechaActual' AND PEDI_ESTADO='A'";
	$arrConsulta = _query($strSub);

	foreach ($arrConsulta as $index => $arrayIndex) {
		foreach ($arrConsulta[$index] as $key3 => $arrayItem3) {
			if(is_numeric($key3)){
				unset($arrConsulta[$index][$key3]);
			}
		}
	}

	if($_GET["admin"]=="S"){
		$arrConsulta = getDetallePedido($arrConsulta);
	}else{
		//return $arrConsulta;
	}
	return $arrConsulta;
}

function getDetallePedido($arrConsulta){
	
	$conexion = _connectDB();
	
	foreach ($arrConsulta as $key => $arrayItem) {
		//return $arrayItem;
		$PEDI_ID =$arrayItem['PEDI_ID'];
		$PEDI_CODNAV =$arrayItem['PEDI_CODNAV'];
		$strSub = "SELECT
		*
	FROM
		SAB.SAB_RRHH_CAFE_PEDI P, SAB.SAB_RRHH_CAFE_PDET D, SAB.SAB_RRHH_CAFE_MDET DM, SAB.SAB_RRHH_CAFE_PCOM DC, SAB.SAB_RRHH_CAFE_MCOM M
	WHERE
		P. PEDI_ID = $PEDI_ID
		AND D.PEDI_ID = P.PEDI_ID
		AND D.MDET_ID = DM.MDET_ID
		AND PEDI_CODNAV='$PEDI_CODNAV'
		AND D.PDET_ID = DC.PDET_ID
		AND DC.MCOM_ID = M.MCOM_ID
		AND MCOM_TIPO='G'";
		$arrSubConsulta = _query($strSub);

		$consulta = oci_parse($conexion, $strSub);
		$r = oci_execute($consulta);
		
		if (!$r){
			$e = oci_error($consulta);
			throw new Exception("Error en detalles de menu personal: ".$e['message']);
		};
		foreach ($arrSubConsulta as $index => $arrayIndex) {
			foreach ($arrSubConsulta[$index] as $key3 => $arrayItem3) {
				if(is_numeric($key3)){
					unset($arrSubConsulta[$index][$key3]);
				}
			}
		}
		
		$arrConsulta[$key]["complementos"]= $arrSubConsulta;

		$arrConsulta[$key]["mdet_plato"]= $arrSubConsulta[0]["MDET_PLATO"];

		$strSub = "SELECT
		*
	FROM
		SAB.SAB_RRHH_CAFE_PEDI P, SAB.SAB_RRHH_CAFE_PDET D, SAB.SAB_RRHH_CAFE_MDET DM, SAB.SAB_RRHH_CAFE_PCOM DC, SAB.SAB_RRHH_CAFE_MCOM M
	WHERE
		P. PEDI_ID = $PEDI_ID
		AND to_char(DM.MDET_DIA,'YYYY-MM-DD')=to_char(SYSDATE,'YYYY-MM-DD')
		AND D.PEDI_ID = P.PEDI_ID
		AND D.MDET_ID = DM.MDET_ID
		AND PEDI_CODNAV='$PEDI_CODNAV'
		AND D.PDET_ID = DC.PDET_ID
		AND DC.MCOM_ID = M.MCOM_ID
		AND MCOM_TIPO='P'";
		$arrSubConsulta = _query($strSub);

		$consulta = oci_parse($conexion, $strSub);
		$r = oci_execute($consulta);
		
		if (!$r){
			$e = oci_error($consulta);
			throw new Exception("Error en detalles de postre: ".$e['message']);
		};
		foreach ($arrSubConsulta as $index => $arrayIndex) {
			foreach ($arrSubConsulta[$index] as $key3 => $arrayItem3) {
				if(is_numeric($key3)){
					unset($arrSubConsulta[$index][$key3]);
				}
			}
		}
		
		$arrConsulta[$key]["postres"]= $arrSubConsulta;
	}

	return $arrConsulta;
}


function getHorarioAsignado($fechaActual,$codemp){
	
	$conexion = _connectDB();
	
	$strSub = "SELECT 
	ID
	, TO_CHAR(START_TIME,'HH24') START_TIME
	, TO_CHAR(END_TIME,'HH24') END_TIME
	, MEALTIME
	FROM 
	TA_SCHEDULE@RELOJSQL
	WHERE 
	USER_ID= regexp_replace(regexp_replace('$codemp', '[^0-9]', ''),'0','',1,1)
	AND to_char(START_TIME,'YYYY-MM-DD')='$fechaActual'";
	$arrConsulta = _query($strSub);

	foreach ($arrConsulta as $index => $arrayIndex) {
		foreach ($arrConsulta[$index] as $key3 => $arrayItem3) {
			if(is_numeric($key3)){
				unset($arrConsulta[$index][$key3]);
			}
		}
	}

	return $arrConsulta[0];
}




function filtrarElementosAll($arrConsulta,$mostrar){
	
		foreach ($arrConsulta as $key => $arrayItem) {
			foreach ($arrConsulta[$key] as $key2 => $arrayItem2) {
				if(!in_array($key2, $mostrar)){
					unset($arrConsulta[$key][$key2]);
				}
			}
		}
		return $arrConsulta;
}

function filtrarElementosFirts($arrConsulta,$mostrar){
		foreach ($arrConsulta as $key => $arrayItem) {
			if(!in_array($key, $mostrar)){
				unset($arrConsulta[$key]);
			}
		}
		return $arrConsulta;
}

function filtrarLlaves($arrConsulta){
	
	$conexion = _connectDB();
	try {
		
		$mostrar = array("GUIA_CORR","GUIA_ANIO","TIPOGUIA_COD","CIA_COD","GUIA_PESO","GUIA_CANTREALPIEZA","DOCUMENTO","GUIA","LINAGEN_NOM","FERE_ID","FERE_ANIO");

		foreach ($arrConsulta as $key => $arrayItem) {
			
			foreach ($arrConsulta[$key] as $key2 => $arrayItem2) {
				if(!in_array($key2, $mostrar)){
					unset($arrConsulta[$key][$key2]);
				}
			}
			

			//$arrConsulta[$key]["REFE_NAC"]= array("pais" =>$arrayItem['PAIS'],"alpha3" =>$arrayItem['ALPHA3']);
		}

		return $arrConsulta;

	}catch(Exception $e) {
		$message = array(
			"error" => $e->getMessage()
		);

		http_response_code(500);
		return json_encode($message);
	}
}

function array_change_key_case_recursive($arr)
{
    return array_map(function($item){
        if(is_array($item))
            $item = array_change_key_case_recursive($item);
        return $item;
    },array_change_key_case($arr));
}


function crearPase($ID_PILOTO){
	$parametros = json_decode(file_get_contents('php://input'));
	$conexion = _connectDB();

		$informacion = $parametros->informacion;
		$REFE_DPI = sanear_string($informacion->REFE_DPI);
		$vehiculo = $parametros->vehiculo;
		$PLACA = sanear_string($vehiculo->PLACA);

		$llavesGuia = $parametros->llavesGuia;
		$GUIA_CORR = sanear_string($llavesGuia->GUIA_CORR);
		$GUIA_ANIO = sanear_string($llavesGuia->GUIA_ANIO);
		$TIPOGUIA_COD = sanear_string($llavesGuia->TIPOGUIA_COD);

		$strConsulta = "
		INSERT INTO SAB.SAB_INTERNET_GUIA_ENV (CIA_COD, ENV_CORR, ENV_ANIO,
			ENV_FEC, ESTADO, PLACA_NO,
			CREADO_POR,ID_PILOTO, PLACA_FURGON, 
			COD_TIPO_VEHICULO, COD_TIPO_PLACA, ID_SOL_PASE)
			VALUES ('ASO',
			SAB.SAB_INTERNET_ENV_SEQ.nextval,
			TO_CHAR(SYSDATE,'YYYY'),
			SYSDATE, 'CREA', '$PLACA',
			USER,$ID_PILOTO, '',
			'', '', '$REFE_DPI')
			RETURNING 
			ENV_CORR,
			ENV_ANIO
            INTO 
                :CORRELATIVO
				,:ENV_ANIO";

		$consulta = oci_parse($conexion, $strConsulta);
		oci_bind_by_name($consulta, ':CORRELATIVO', $theNewKRID, 8);
		oci_bind_by_name($consulta, ':ENV_ANIO', $ENV_ANIO, 8);
		$r= oci_execute($consulta);
		if (!$r){
			$e = oci_error($consulta);
			throw new Exception($e['message']);
		}

		$strConsulta = "
		INSERT INTO SAB.SAB_INTERNET_BOL (CIA_COD, BOL_CORR, GUIA_I_CORR, 
                               GUIA_I_ANIO, TIPOGUIA_I_COD, ENV_ANIO,
                               ENV_CORR, PIEZAS, ESTADO, 
                               FAC_OPER, CED_ORDEN, ID_PILOTO_RECIBE,
							   REQUE_SERIE, REQUE_CORR, REQUE_ANIO)
                        VALUES ('ASO', SAB.SAB_INTERNET_BOL_SEQ.NEXTVAL, $GUIA_CORR, 
                                $GUIA_ANIO, '$TIPOGUIA_COD', $ENV_ANIO, 
                                $theNewKRID, 0, 'CREA',  
                                '', '', $ID_PILOTO,
								'', '', '')";

		$consulta = oci_parse($conexion, $strConsulta);
		$r= oci_execute($consulta);
		if (!$r){
			$e = oci_error($consulta);
			throw new Exception("Error creando pase: ".$e['message']);
		}

		$data = array(
			"ENV_ANIO"=>$ENV_ANIO,
			"ENV_CORR"=>$theNewKRID
		);
		return json_encode($data,JSON_NUMERIC_CHECK);
}



function obtiene_detpedido_seq()
{
	$strConsulta = "SELECT
						SAB.SAB_RRHH_CAFE_PDET_SEQ1.NEXTVAL
					FROM
						DUAL";

	$conexion = _connectDB();

	$consulta = oci_parse($conexion, $strConsulta);
	oci_execute($consulta);

	$result = "";
	while($arrTMP = oci_fetch_array($consulta)){
		$result = $arrTMP[0];
	}

	oci_close($conexion);
	return $result;
}

function crearPedido(){
	$parametros = json_decode(file_get_contents('php://input'));
	$conexion = _connectDB();
	try {
		$menu = $parametros;
		$menu_id = sanear_string($menu->menu_id);
		$mdet_id = sanear_string($menu->mdet_id);
		$codemp = sanear_string($menu->codemp);
		$mcom_id = sanear_string($menu->mcom_id);

		$codigo = limpiarCodemp($codemp);
		
		$strConsulta = "
		INSERT INTO SAB.SAB_RRHH_CAFE_PEDI
			(
			PEDI_ID,
			PEDI_CODNAV,
			PEDI_FECHA,
			PEDI_ESTADO,
			MENU_ID,
			PEDI_USUARIO
			)
			VALUES
			(
			( SELECT (NVL(MAX(PEDI_ID),0) + 1) FROM SAB.SAB_RRHH_CAFE_PEDI),
			'$codemp',
			sysdate,
			'A',
			'$menu_id',
			'SYS_CAFE'
			)
			RETURNING 
			PEDI_ID
            INTO 
                :PEDI_ID";

		$consulta = oci_parse($conexion, $strConsulta);
		oci_bind_by_name($consulta, ':PEDI_ID', $PEDI_ID, 8);
		$r= oci_execute($consulta);
		if (!$r){
			$e = oci_error($consulta);
			throw new Exception("Error creando pedido: ".$e['message']);
		}


		$detped_id = obtiene_detpedido_seq();

		$strConsulta = "
		INSERT
							INTO SAB.SAB_RRHH_CAFE_PDET
						(
							PDET_ID,
							PEDI_ID,
							MDET_ID
						)
						VALUES
						(
							$detped_id,
							$PEDI_ID,
							$mdet_id
						)
						RETURNING 
						PDET_ID
            			INTO 
                		:PDET_ID";

		$consulta = oci_parse($conexion, $strConsulta);
		$r = oci_execute($consulta);
		
		$consulta = oci_parse($conexion, $strConsulta);
		oci_bind_by_name($consulta, ':PDET_ID', $PDET_ID, 8);
		$r= oci_execute($consulta);
		if (!$r){
			$e = oci_error($consulta);
			throw new Exception("Error creando detalle de platillo: ".$e['message']);
		}


		$complementos = $parametros->complementos;
		$postres = $parametros->postres;
		foreach ($complementos as $Cont){

			if($Cont->pedir==="true"){

				$mcom_id = $Cont->mcom_id;
						
				$strConsulta = "
				INSERT
				INTO SAB.SAB_RRHH_CAFE_PCOM
				(
					PCOM_ID,
					PDET_ID,
					MCOM_ID
				)
				VALUES
				(
					( SELECT (NVL(MAX(PCOM_ID),0) + 1) FROM SAB.SAB_RRHH_CAFE_PCOM),
					$PDET_ID,
					$mcom_id
				)";


				$consulta = oci_parse($conexion, $strConsulta);
				$r= oci_execute($consulta);
				if (!$r){
				$e = oci_error($consulta);
				throw new Exception("Error creando complemento: ".$e['message']);
				}
			};
			
		}

		foreach ($postres as $Cont){

			if($Cont->pedir==="true"){
				
				$mcom_id = $Cont->mcom_id;
						
				$strConsulta = "
				INSERT
				INTO SAB.SAB_RRHH_CAFE_PCOM
				(
					PCOM_ID,
					PDET_ID,
					MCOM_ID
				)
				VALUES
				(
					( SELECT (NVL(MAX(PCOM_ID),0) + 1) FROM SAB.SAB_RRHH_CAFE_PCOM),
					$PDET_ID,
					$mcom_id
				)";


				$consulta = oci_parse($conexion, $strConsulta);
				$r= oci_execute($consulta);
				if (!$r){
				$e = oci_error($consulta);
				throw new Exception("Error creando postre: ".$e['message']);
				}
			};
			
		}

		$message = array(
			"message" => "Se ha guardado el pedido."
		);
		
		return json_encode($message,JSON_NUMERIC_CHECK);
	}catch(Exception $e) {
		$message = array(
			"error" => $e->getMessage()//." - <h5>Codigo de error</h5> ".$e->getLine()
		);

		//http_response_code(500);
		return json_encode($message);
	}
}

function limpiarCodemp($codigo)
{
	$strConsulta = "SELECT regexp_replace(regexp_replace('$codigo', '[^0-9]', ''),'0','',1,1) FROM DUAL";

	$conexion = _connectDB();

	$consulta = oci_parse($conexion, $strConsulta);
	oci_execute($consulta);

	$result = "";
	while($arrTMP = oci_fetch_array($consulta)){
		$result = $arrTMP[0];
	}

	oci_close($conexion);
	return $result;
}