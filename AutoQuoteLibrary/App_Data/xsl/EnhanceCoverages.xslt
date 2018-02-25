<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
	<xsl:param name="premium1"></xsl:param>
	<xsl:param name="premium2"></xsl:param>
	<xsl:param name="premium3"></xsl:param>
	<xsl:output method="xml" encoding="UTF-8" indent="yes" omit-xml-declaration="yes"/>
	<xsl:template match="/">
		<xsl:variable name="var1_enhanceCoverageState" select="."/>
		<table id="Table4" cellpadding="0" width="100%" border="0" class="qbcTables">
			<tr>
				<td colspan="4">
					<h3>Enhanced Coverages</h3>
				</td>
			</tr>
			<tr class="coverageHdline">
				<td width="35%">Coverage</td>
				<td width="38%">Limits</td>
				<td width="15%">Applies To</td>
				<td width="10%">Premiums</td>
			</tr>

			<xsl:for-each select="$var1_enhanceCoverageState/State/EnhanceCoverage">
				<xsl:variable name="var_EnhanceCoverage" select="."/>
				
				<tr>
					<td>
						<xsl:choose>
							<xsl:when test="contains(@name, 'bundle')">
								<!--SJS PRD12517 - added disable-output-escaping to both options below-->
								<xsl:value-of select="string($var_EnhanceCoverage/@name)" disable-output-escaping="yes"/>
							</xsl:when>
							<xsl:otherwise>
								<xsl:value-of select="string($var_EnhanceCoverage/@name)" disable-output-escaping="yes"/>
							</xsl:otherwise>
						</xsl:choose>
					</td>
					<td>
						<xsl:value-of select="string($var_EnhanceCoverage/@Limits)"/>
					</td>
					<td>
						<xsl:value-of select="string($var_EnhanceCoverage/@applyTo)"/>
					</td>
					<td class="premCalc">
						<xsl:choose>
							<!--jrenz #7188 10/11/2011-->
							<xsl:when test="@prev='TeachersTenureTest'" >
								Included at No Charge
							</xsl:when>
							<xsl:when test="@prev= 'bundle1Test' and $premium1 !=''">
								<xsl:value-of select="format-number($premium1, '$###,###,##0.00')"/>
							</xsl:when>
							<xsl:when test="@prev= 'bundle2Test' and $premium2 !=''" >
								<xsl:value-of select="format-number($premium2, '$###,###,##0.00')"/>
							</xsl:when>
							<xsl:when test="@prev= 'bundle3Test' and $premium3 != '' ">
								<xsl:value-of select="format-number($premium3, '$###,###,##0.00')"/>
							</xsl:when>
						</xsl:choose>
					</td>
				</tr>
				<xsl:if test="@prev='DependentProtTest'">
					<xsl:for-each select="$var1_enhanceCoverageState/State/EnhanceCoverage/DependentProtTest">
						<xsl:variable name="var_DependentProt" select="."/>
						<tr>
							<td>
								<xsl:value-of select="string($var_DependentProt/@name)"/>
							</td>
							<td>
								<xsl:value-of select="string($var_DependentProt/@Limits)"/>
							</td>
							<td colspan="2"></td>
						</tr>
					</xsl:for-each>
				</xsl:if>
			</xsl:for-each>
			<tr>
				<td colspan="4"></td>
			</tr>
			<!--tr>
			<td colspan="4" class="smtext" >The above premium reflects only the cost of the requested changes. There may be additional state mandated fees attached to this amount.
			</td>
		</tr-->
		
	</table>
	</xsl:template>
</xsl:stylesheet>
