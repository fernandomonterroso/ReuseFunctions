create or replace FUNCTION F_FORMATO_NIT(P_NIT VARCHAR2) RETURN VARCHAR2 AS 
V_TEMP VARCHAR2(20);
V_RESULT VARCHAR2(20);
BEGIN
  V_RESULT := 'N';
  IF (P_NIT <> 'C.F.') THEN
    IF (P_NIT = 'CF') THEN
        V_RESULT := 'S';
        RETURN V_RESULT;
    END IF;
    SELECT REGEXP_SUBSTR(P_NIT, '^[0-9]+-[0-9K]$')
    --SELECT REGEXP_SUBSTR(REPLACE(P_NIT,'-',''), '^[0-9]+[0-9K]$')
    INTO V_TEMP
    FROM 
    DUAL;
    IF (V_TEMP IS NOT NULL) THEN
      IF (F_VALIDA_NIT(P_NIT) = 'S') THEN
      --IF (F_VALIDA_NIT(V_TEMP) = 'S') THEN
        V_RESULT := 'S';
       END IF;
    END IF;
  ELSE
    V_RESULT := 'S';
  END IF;
  RETURN V_RESULT;
END F_FORMATO_NIT;
