# User guide

## Calculate a configuration

1. Enter numeric values in the module configuration panel.
2. Keep the units shown in the panel in mind; values are entered as plain
   numbers.
3. Select **Calculate**.
4. Review the KPI values, thermal status, and loss chart.

Invalid fields are reported together so they can be corrected in one pass.

## Save and load

1. Enter a JSON path in **Project file**.
2. Select **Save project**.
3. Later, enter the same path and select **Load project**.

Project files include schema version `1`, inputs, and the last calculated
result. Loading validates the stored input before applying it.

## Export results

After a successful calculation, enter a CSV path in **Export results** and
select **Export CSV**. The file contains metric names, values, and units.

## Compare configurations

1. Save a second configuration as another JSON project file.
2. Put its path in **Configuration B project file**.
3. Select **Compare projects**.

The comparison reports whether one configuration dominates on total loss and
thermal margin. It also shows how total loss changes with switching frequency.
