<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="xs">
	<xsl:output method="xml" encoding="UTF-8" indent="yes" omit-xml-declaration="yes"/>
	<xsl:template match="/">
		<xsl:variable name="var1_instance_CovState" select="."/>
		<Bundles>
			<xsl:for-each select="$var1_instance_CovState/State">
				<xsl:variable name="var_State1" select="."/>
				<EnhancedCoverages>
					<xsl:for-each select="$var_State1/EnhanceCoverage[starts-with(@prev,'bundle')]">
						<xsl:variable name="var_cvg1" select="."/>
						<xsl:variable name="cov_name1" select="string(substring-before($var_cvg1/@prev, 'Test'))"/>
						<EnhancedCoverage>
							<Name>
								<xsl:value-of select="string($var_cvg1/@name)"/>
							</Name>
							<Desc>
								<!--SJS PRD12517 - added disable-output-->
								<xsl:value-of select="string($var_cvg1/@desc)" disable-output-escaping="yes"/>
							</Desc>
							<Purchased>
								<xsl:choose>
									<xsl:when test="$var_cvg1/@premium > 0">YES</xsl:when>
									<xsl:otherwise>NO</xsl:otherwise>
								</xsl:choose>
								<!--SJS PRD12517 - added disable-output-->
								<xsl:value-of select="string($var_cvg1/@faq)" disable-output-escaping="yes"/>
							</Purchased>
							<CovCode>
								<xsl:value-of select="translate($cov_name1,'bundle','Bundle')"/>
							</CovCode>
							<QuoteProperty>
								<xsl:value-of select="string($var_cvg1/@prev)"/>
							</QuoteProperty>
						</EnhancedCoverage>
					</xsl:for-each>
				</EnhancedCoverages>
			</xsl:for-each>
			<xsl:for-each select="$var1_instance_CovState/State">
				<xsl:variable name="var_State2" select="."/>
				<EnhancedPremiums>
					<xsl:for-each select="$var_State2/EnhanceCoverage[starts-with(@prev,'bundle')]">
						<xsl:variable name="var_cvg2" select="."/>
						<xsl:variable name="cov_name2" select="string(substring-before($var_cvg2/@prev, 'Test'))"/>
						<Premium>
							<Name>
								<xsl:value-of select="string($var_cvg2/@name)"/>
							</Name>
							<CovCode>
								<xsl:value-of select="translate($cov_name2,'bundle','Bundle')"/>
							</CovCode>
							<Amount>
								<xsl:value-of select="string($var_cvg2/@premium)"/>
							</Amount>
						</Premium>
					</xsl:for-each>
				</EnhancedPremiums>
			</xsl:for-each>
		</Bundles>
	</xsl:template>
</xsl:stylesheet>
