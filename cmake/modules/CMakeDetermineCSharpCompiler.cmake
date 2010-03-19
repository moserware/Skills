# copyright (c) 2007, 2009 Arno Rehn arno@arnorehn.de
# copyright (c) 2008 Helio castro helio@kde.org
#
# Redistribution and use is allowed according to the terms of the GPL license.

# determine the compiler to use for C# programs
# NOTE, a generator may set CMAKE_CSharp_COMPILER before
# loading this file to force a compiler.

if(NOT CMAKE_CSharp_COMPILER)
    # prefer the environment variable CSC
    if($ENV{CSC} MATCHES ".+")
        if (EXISTS $ENV{CSC})
            message(STATUS "Found compiler set in environment variable CSC: $ENV{CSC}.")
            set(CMAKE_CSharp_COMPILER $ENV{CSC})
        else (EXISTS $ENV{CSC})
            message(SEND_ERROR "Could not find compiler set in environment variable CSC:\n$ENV{CSC}.")
        endif (EXISTS $ENV{CSC})
    endif($ENV{CSC} MATCHES ".+")

    # if no compiler has been specified yet, then look for one
    if (NOT CMAKE_CSharp_COMPILER)
        find_package(Mono)
        set (CMAKE_CSharp_COMPILER "${GMCS_EXECUTABLE}")

        # still not found, try csc.exe
        if (NOT CMAKE_CSharp_COMPILER)
            get_filename_component(dotnet_path "[HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\.NETFramework;InstallRoot]" PATH)
            find_program(CMAKE_CSharp_COMPILER NAMES csc PATHS "${dotnet_path}/Framework/v2.0.50727")
            file(TO_NATIVE_PATH "${dotnet_path}/Framework/v2.0.50727" native_path)
            message(STATUS "Looking for csc: ${CMAKE_CSharp_COMPILER}")

            # give up
            if (NOT CMAKE_CSharp_COMPILER)
                message (STATUS "Couldn't find a valid C# compiler. Set either CMake_CSharp_COMPILER or the CSC environment variable to a valid path.")
            endif (NOT CMAKE_CSharp_COMPILER)
        endif (NOT CMAKE_CSharp_COMPILER)
    endif (NOT CMAKE_CSharp_COMPILER)

endif(NOT CMAKE_CSharp_COMPILER)

# now try to find the gac location
if (CMAKE_CSharp_COMPILER AND NOT GAC_DIR AND MONO_FOUND)
    find_package(PkgConfig)

    if (PKG_CONFIG_FOUND)
        pkg_search_module(MONO_CECIL mono-cecil)
        if(MONO_CECIL_FOUND)
            execute_process(COMMAND ${PKG_CONFIG_EXECUTABLE} mono-cecil --variable=assemblies_dir OUTPUT_VARIABLE GAC_DIR OUTPUT_STRIP_TRAILING_WHITESPACE)
        endif(MONO_CECIL_FOUND)

        pkg_search_module(CECIL cecil)
        if(CECIL_FOUND)
            execute_process(COMMAND ${PKG_CONFIG_EXECUTABLE} cecil --variable=assemblies_dir OUTPUT_VARIABLE GAC_DIR OUTPUT_STRIP_TRAILING_WHITESPACE)
        endif(CECIL_FOUND)

        if (NOT GAC_DIR)
            execute_process(COMMAND ${PKG_CONFIG_EXECUTABLE} mono --variable=libdir OUTPUT_VARIABLE MONO_LIB_DIR OUTPUT_STRIP_TRAILING_WHITESPACE)
            if (MONO_LIB_DIR)
                set (GAC_DIR "${MONO_LIB_DIR}/mono")
                message (STATUS "Could not find cecil, guessing GAC dir from mono prefix: ${GAC_DIR}")
            endif (MONO_LIB_DIR)
        endif (NOT GAC_DIR)
    endif (PKG_CONFIG_FOUND)

    if (NOT GAC_DIR)
        set (GAC_DIR "/usr/lib/mono")
        message(STATUS "Could not find cecil or mono. Using default GAC dir: ${GAC_DIR}")
    endif (NOT GAC_DIR)
endif (CMAKE_CSharp_COMPILER AND NOT GAC_DIR AND MONO_FOUND)

# Create a cache entry so the user can modify this.
set(GAC_DIR "${GAC_DIR}" CACHE PATH "Location of the GAC")
message(STATUS "Using GAC dir: ${GAC_DIR}")

mark_as_advanced(CMAKE_CSharp_COMPILER)

if (CMAKE_CSharp_COMPILER)
    set (CMAKE_CSharp_COMPILER_LOADED 1)
endif (CMAKE_CSharp_COMPILER)

# configure variables set in this file for fast reload later on
configure_file(${CMAKE_SOURCE_DIR}/cmake/modules/CMakeCSharpCompiler.cmake.in 
  ${CMAKE_BINARY_DIR}${CMAKE_FILES_DIRECTORY}/CMakeCSharpCompiler.cmake IMMEDIATE @ONLY)
set(CMAKE_CSharp_COMPILER_ENV_VAR "CSC")
