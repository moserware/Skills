# - Try to find the mono, mcs, gmcs and gacutil
#
# defines
#
# MONO_FOUND - system has mono, mcs, gmcs and gacutil
# MONO_PATH - where to find 'mono'
# GMCS_PATH - where to find 'gmcs'
# GACUTIL_PATH - where to find 'gacutil'
#
# copyright (c) 2007 Arno Rehn arno@arnorehn.de
#
# Redistribution and use is allowed according to the terms of the GPL license.

FIND_PROGRAM (MONO_EXECUTABLE mono)
FIND_PROGRAM (GMCS_EXECUTABLE gmcs)
FIND_PROGRAM (GACUTIL_EXECUTABLE gacutil)

SET (MONO_FOUND FALSE CACHE INTERNAL "")

IF (MONO_EXECUTABLE AND GMCS_EXECUTABLE AND GACUTIL_EXECUTABLE)
	SET (MONO_FOUND TRUE CACHE INTERNAL "")
ENDIF (MONO_EXECUTABLE AND GMCS_EXECUTABLE AND GACUTIL_EXECUTABLE)

IF (NOT Mono_FIND_QUIETLY)
    MESSAGE(STATUS "Path of mono: ${MONO_EXECUTABLE}")
    MESSAGE(STATUS "Path of gmcs: ${GMCS_EXECUTABLE}")
    MESSAGE(STATUS "Path of gacutil: ${GACUTIL_EXECUTABLE}")
ENDIF (NOT Mono_FIND_QUIETLY)

IF (NOT MONO_FOUND)
	IF (Mono_FIND_REQUIRED)
		MESSAGE(FATAL_ERROR "Could not find one or more of the following programs: mono, gmcs, gacutil")
	ENDIF (Mono_FIND_REQUIRED)
ENDIF (NOT MONO_FOUND)

MARK_AS_ADVANCED(MONO_EXECUTABLE GMCS_EXECUTABLE GACUTIL_EXECUTABLE)
