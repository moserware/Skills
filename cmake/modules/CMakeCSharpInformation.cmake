# copyright (c) 2007, 2009 Arno Rehn arno@arnorehn.de
# copyright (c) 2008 Helio castro helio@kde.org
#
# Redistribution and use is allowed according to the terms of the GPL license.

# This file adds support for the C# language to cmake.
#
# It adds the following functions:
#
# csharp_add_executable (<name> <source files> [UNSAFE] [WINEXE] [REFERENCES <references>]
#                        [COMPILE_FLAGS <flags to be passed to the compiler>]
#                        [COMPILE_DEFINITIONS <additional definitions>] )
#
# csharp_add_library (<name> <source files> [UNSAFE] [REFERENCES <references>]
#                     [COMPILE_FLAGS <flags to be passed to the compiler>]
#                     [COMPILE_DEFINITIONS <additional definitions>] )
#
# install_assembly (<target name> DESTINATION <assembly destination directory>
#                   [PACKAGE <package name>] )
# The assembly destination directory is only used if we compile with Visual C# and thus can't use gacutil.
# If a package is specified and a file called <target>.pc.cmake exists in the current source directory,
# this function will configure the template file. All occurences of @assembly@ will be replaced with
# the path to the assembly. The resulting <target>.pc file will be installed to
# <CMAKE_INSTALL_PREFIX>/lib/pkgconfig/ . If you want to have a different basename for the template file,
# set the 'pkg-config_template_basename' property of the target with set_property.
#
# Example:
# ------------------------------
# cmake code:
# ------------------------------
# csharp_add_library(foo foo.cs)
# install_assembly(foo DESTINATION lib)
#
# ------------------------------
# contents of foo.pc.cmake file:
# ------------------------------
# Name: Foo
# Description: Foo library
# Version: 1.0
# Libs: -r:@assembly@

# ----- support macros -----
macro(GET_LIBRARY_OUTPUT_DIR var)
    if (NOT LIBRARY_OUTPUT_PATH)
        set(${var} ${CMAKE_CURRENT_BINARY_DIR})
    else (NOT LIBRARY_OUTPUT_PATH)
        set(${var} ${LIBRARY_OUTPUT_PATH})
    endif (NOT LIBRARY_OUTPUT_PATH)
endmacro(GET_LIBRARY_OUTPUT_DIR)

macro(GET_EXECUTABLE_OUTPUT_DIR var)
    if (NOT EXECUTABLE_OUTPUT_PATH)
        set(${var} ${CMAKE_CURRENT_BINARY_DIR})
    else (NOT EXECUTABLE_OUTPUT_PATH)
        set(${var} ${EXECUTABLE_OUTPUT_PATH})
    endif (NOT EXECUTABLE_OUTPUT_PATH)
endmacro(GET_EXECUTABLE_OUTPUT_DIR)

# This does just not always work... why?!
# macro(MAKE_PROPER_FILE_LIST var)
#     foreach(file ${ARGN})
#         if (IS_ABSOLUTE "${file}")
#             file(GLOB globbed "${file}")
#         else (IS_ABSOLUTE "${file}")
#             file(GLOB globbed "${CMAKE_CURRENT_SOURCE_DIR}/${file}")
#         endif (IS_ABSOLUTE "${file}")
#         
#         foreach (glob ${globbed})
#             file(TO_NATIVE_PATH "${glob}" native)
#             list(APPEND proper_file_list "${native}")
#         endforeach (glob ${globbed})
#     endforeach(file ${ARGN})
# endmacro(MAKE_PROPER_FILE_LIST)

# ----- actual functions -----

# ----- add an executable -----
function(csharp_add_executable target)
    set(current "s")
    set(dotnet_target "exe")

    foreach (arg ${ARGN})
        file(TO_NATIVE_PATH ${arg} native_path)

        if (arg STREQUAL "UNSAFE")
            set (unsafe "/unsafe")
        elseif (arg STREQUAL "WINEXE")
            set (dotnet_target "winexe")
        elseif (arg STREQUAL "REFERENCES")
            set (current "r")
        elseif (arg STREQUAL "COMPILE_FLAGS")
            set (current "flags")
        elseif (arg STREQUAL "COMPILE_DEFINITIONS")
            set (current "defs")
        else (arg STREQUAL "UNSAFE")
            if (current STREQUAL "s")
                # source file
                list(APPEND sources ${native_path})
            elseif (current STREQUAL "r")
                # reference
                if (TARGET ${arg})
                    # this is an existing target - get the target assembly
                    get_property(prop TARGET ${arg} PROPERTY _assembly)
                    list(APPEND references "/r:${prop}")
                    list(APPEND deps ${arg})
                else (TARGET ${arg})
                    # something different (e.g. assembly name in the gac)
                    list(APPEND references "/r:${native_path}")
                endif (TARGET ${arg})
            elseif (current STREQUAL "flags")
                list(APPEND _csc_opts "${arg}")
            elseif (current STREQUAL "defs")
                list(APPEND _csc_opts "/define:${arg}")
            endif (current STREQUAL "s")
        endif (arg STREQUAL "UNSAFE")
    endforeach (arg ${ARGN})

    if (CMAKE_BUILD_TYPE STREQUAL "Debug")
        list(APPEND _csc_opts "/define:DEBUG")
        list(APPEND _csc_opts "/debug")
    endif (CMAKE_BUILD_TYPE STREQUAL "Debug")

    get_executable_output_dir(outdir)
    if (NOT IS_ABSOLUTE "${outdir}")
        message(FATAL_ERROR "Directory \"${outdir}\" is not an absolute path!")
    endif (NOT IS_ABSOLUTE "${outdir}")

    file(RELATIVE_PATH relative_path "${CMAKE_BINARY_DIR}" "${outdir}/${target}.exe")
    file(TO_NATIVE_PATH "${outdir}/${target}" native_target)
    
    # inlined - this doesn't work as a macro :(
    foreach(file ${sources})
        file(TO_CMAKE_PATH "${file}" cmake_file)
        
        if (IS_ABSOLUTE "${cmake_file}")
            file(GLOB globbed "${cmake_file}")
        else (IS_ABSOLUTE "${cmake_file}")
            file(GLOB globbed "${CMAKE_CURRENT_SOURCE_DIR}/${cmake_file}")
        endif (IS_ABSOLUTE "${cmake_file}")
        
        foreach (glob ${globbed})
            file(TO_CMAKE_PATH "${glob}" cmake_path)
            list(APPEND cmake_file_list "${cmake_path}")
        endforeach (glob ${globbed})
        if (NOT globbed)
            list(APPEND cmake_file_list "${cmake_file}")
        endif (NOT globbed)
        list(APPEND compiler_file_list ${file})
    endforeach(file ${sources})

    get_directory_property(compile_definitions COMPILE_DEFINITIONS)
    foreach (def ${compile_definitions})
        # macros with values aren't supported by C#
        if (NOT def MATCHES ".*=.*")
            list(APPEND _csc_opts "/define:${def}")
        endif (NOT def MATCHES ".*=.*")
    endforeach (def ${compile_definitions})

    get_directory_property(link_dirs LINK_DIRECTORIES)
    foreach (dir ${link_dirs})
        list(APPEND _csc_opts "/lib:${dir}")
    endforeach (dir ${link_dirs})

    add_custom_command(OUTPUT "${outdir}/${target}.stubexe"
                       COMMAND "${CMAKE_COMMAND}" -E make_directory "${outdir}" # create the output dir
                       COMMAND "${CMAKE_CSharp_COMPILER}" /nologo /target:${dotnet_target} "/out:${native_target}.exe" # build the executable
                                                        ${_csc_opts} ${unsafe} ${references} ${compiler_file_list}
                       COMMAND "${CMAKE_COMMAND}" -E touch "${outdir}/${target}.stubexe" # create the stub so that DEPENDS will work
                       WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}" # working directory is the source directory, so we don't have to care about relative paths
                       DEPENDS ${cmake_file_list}
                       COMMENT "Building ${relative_path}" VERBATIM) # nice comment
    add_custom_target(${target} ALL DEPENDS "${outdir}/${target}.stubexe" SOURCES ${cmake_file_list}) # create the actual target
    if (deps)
        add_dependencies(${target} ${deps})
    endif(deps)
endfunction(csharp_add_executable)

# ----- add a library -----
function(csharp_add_library target)
    set(current "s")
    
    foreach (arg ${ARGN})
        file(TO_NATIVE_PATH ${arg} native_path)
        
        if (arg STREQUAL "UNSAFE")
            set (unsafe "/unsafe")
        elseif (arg STREQUAL "REFERENCES")
            set (current "r")
        elseif (arg STREQUAL "COMPILE_FLAGS")
            set (current "flags")
        elseif (arg STREQUAL "COMPILE_DEFINITIONS")
            set (current "defs")
        else (arg STREQUAL "UNSAFE")
            if (current STREQUAL "s")
                # source file
                list(APPEND sources ${native_path})
            elseif (current STREQUAL "r")
                # reference
                if (TARGET ${arg})
                    # this is an existing target - get the target assembly
                    get_property(prop TARGET ${arg} PROPERTY _assembly)
                    list(APPEND references "/r:${prop}")
                    list(APPEND deps ${arg})
                else (TARGET ${arg})
                    # something different (e.g. assembly name in the gac)
                    list(APPEND references "/r:${native_path}")
                endif (TARGET ${arg})
            elseif (current STREQUAL "flags")
                list(APPEND _csc_opts "${arg}")
            elseif (current STREQUAL "defs")
                list(APPEND _csc_opts "/define:${arg}")
            endif (current STREQUAL "s")
        endif (arg STREQUAL "UNSAFE")
    endforeach (arg ${ARGN})

    if (CMAKE_BUILD_TYPE STREQUAL "Debug")
        list(APPEND _csc_opts "/define:DEBUG")
        list(APPEND _csc_opts "/debug")
    endif (CMAKE_BUILD_TYPE STREQUAL "Debug")

    get_library_output_dir(outdir)
    if (NOT IS_ABSOLUTE "${outdir}")
        message(FATAL_ERROR "Directory \"${outdir}\" is not an absolute path!")
    endif (NOT IS_ABSOLUTE "${outdir}")

    file(RELATIVE_PATH relative_path "${CMAKE_BINARY_DIR}" "${outdir}/${target}.dll")
    file(TO_NATIVE_PATH "${outdir}/${target}" native_target)
    
    # inlined - this doesn't work as a macro :(
    foreach(file ${sources})
        file(TO_CMAKE_PATH "${file}" cmake_file)
        
        if (IS_ABSOLUTE "${cmake_file}")
            file(GLOB globbed "${cmake_file}")
        else (IS_ABSOLUTE "${cmake_file}")
            file(GLOB globbed "${CMAKE_CURRENT_SOURCE_DIR}/${cmake_file}")
        endif (IS_ABSOLUTE "${cmake_file}")
        
        foreach (glob ${globbed})
            file(TO_CMAKE_PATH "${glob}" cmake_path)
            list(APPEND cmake_file_list "${cmake_path}")
        endforeach (glob ${globbed})
        if (NOT globbed)
            list(APPEND cmake_file_list "${cmake_file}")
        endif (NOT globbed)
        list(APPEND compiler_file_list ${file})
    endforeach(file ${sources})

#     message("CMake File List for target ${target}: ${cmake_file_list}")

    get_directory_property(compile_definitions COMPILE_DEFINITIONS)
    foreach (def ${compile_definitions})
        # macros with values aren't supported by C#
        if (NOT def MATCHES ".*=.*")
            list(APPEND _csc_opts "/define:${def}")
        endif (NOT def MATCHES ".*=.*")
    endforeach (def ${compile_definitions})

    get_directory_property(link_dirs LINK_DIRECTORIES)
    foreach (dir ${link_dirs})
        list(APPEND _csc_opts "/lib:${dir}")
    endforeach (dir ${link_dirs})

    add_custom_command(OUTPUT "${outdir}/${target}.dll"
                       COMMAND "${CMAKE_COMMAND}" -E make_directory "${outdir}" # create the output dir
                       COMMAND "${CMAKE_CSharp_COMPILER}" /nologo /target:library "/out:${native_target}.dll" # build the executable
                                                        ${_csc_opts} ${unsafe} ${references} ${compiler_file_list}
                       WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}" # working directory is the source directory, so we don't have to care about relative paths
                       DEPENDS ${cmake_file_list}
                       COMMENT "Building ${relative_path}" VERBATIM) # nice comment
    add_custom_target(${target} ALL DEPENDS "${outdir}/${target}.dll" SOURCES ${cmake_file_list}) # create the actual target
    set_property(TARGET ${target} PROPERTY _assembly "${native_target}.dll")
    if (deps)
        add_dependencies(${target} ${deps})
    endif(deps)
endfunction(csharp_add_library)

# ----- install a library assembly -----
function(install_assembly target DESTINATION destination_dir)
    # retrieve the absolute path of the generated assembly
    get_property(filename TARGET ${target} PROPERTY _assembly)
    get_property(pc_file TARGET ${target} PROPERTY pkg-config_template_basename)
    if (NOT pc_file)
        set (pc_file ${target})
    endif (NOT pc_file)

    if (NOT filename)
        message(FATAL_ERROR "Couldn't retrieve the assembly filename for target ${target}! Are you sure the target is a .NET library assembly?")
    endif (NOT filename)

    if (NOT MONO_FOUND)
        install(FILES "${filename}" DESTINATION ${destination_dir})
        if (EXISTS "${filename}.config")
            install(FILES "${filename}.config" DESTINATION ${destination_dir})
        endif (EXISTS "${filename}.config")
        return()
    endif (NOT MONO_FOUND)

    if (ARGV3 STREQUAL "PACKAGE" AND ARGV4)
        set (package_option "-package ${ARGV4}")

        if (EXISTS "${CMAKE_CURRENT_SOURCE_DIR}/${pc_file}.pc.cmake")
            set(assembly "${GAC_DIR}/${ARGV4}/${target}.dll")
            configure_file ("${CMAKE_CURRENT_SOURCE_DIR}/${pc_file}.pc.cmake" "${CMAKE_CURRENT_BINARY_DIR}/${pc_file}.pc")

            if (NOT LIB_INSTALL_DIR)
                set (LIB_INSTALL_DIR ${CMAKE_INSTALL_PREFIX}/lib)
            endif (NOT LIB_INSTALL_DIR)
            install(FILES "${CMAKE_CURRENT_BINARY_DIR}/${pc_file}.pc" DESTINATION ${LIB_INSTALL_DIR}/pkgconfig)
        endif (EXISTS "${CMAKE_CURRENT_SOURCE_DIR}/${pc_file}.pc.cmake")

    endif (ARGV3 STREQUAL "PACKAGE" AND ARGV4)

    # So we have the mono runtime and we can use gacutil (it has the -root option, which the MS version doesn't have).
    install(CODE "execute_process(COMMAND ${GACUTIL_EXECUTABLE} -i ${filename} ${package_option} -root ${CMAKE_CURRENT_BINARY_DIR}/tmp_gac)")
    file(REMOVE_RECURSE ${CMAKE_CURRENT_BINARY_DIR}/tmp_gac/mono)
    file(MAKE_DIRECTORY ${CMAKE_CURRENT_BINARY_DIR}/tmp_gac/mono)
    install(DIRECTORY ${CMAKE_CURRENT_BINARY_DIR}/tmp_gac/mono/ DESTINATION ${GAC_DIR} )
endfunction(install_assembly)

set(CMAKE_CSharp_INFORMATION_LOADED 1)
