<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output omit-xml-declaration="yes" indent="no"/>

	<xsl:template match="/">
	<xsl:copy>
		<xsl:apply-templates />
		</xsl:copy>
	</xsl:template>


	<!-- Vehicle Coverages - Remove any vehicles that do not have a sort order -->
	<xsl:template match="VehicleCoverages">
			<xsl:copy>
				<xsl:apply-templates >
					<!-- Sort the Vehicles -->
					<xsl:sort select="SortOrder"></xsl:sort>
				</xsl:apply-templates>
			</xsl:copy>
	</xsl:template>
	
	<xsl:template match="Vehicle">
		<xsl:if test="SortOrder > 0">
			<xsl:copy>
				<xsl:apply-templates />
			</xsl:copy>
		</xsl:if>
	</xsl:template>

	<!-- Policy Coverages - let them all pass by -->
<!-- 	<xsl:template match="PolicyCoverages | node()"> -->
	<xsl:template match="@* | node()">
		<xsl:copy>
			<!-- <xsl:apply-templates select="@* | node()"/> -->
				<xsl:apply-templates />
		</xsl:copy>
	</xsl:template>
	
	
	<!--
		<xsl:variable name="sortOrder" select='SortOrder' />
		<MySort>{$sortOrder}</MySort>
		-->
</xsl:stylesheet>
