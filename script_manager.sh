#!/bin/bash
WINUSER=$(cmd.exe /c echo %USERNAME%)
SEScripts="/mnt/c/Users/${WINUSER::-1}/AppData/Roaming/SpaceEngineers/IngameScripts/local"
CLM="SESM >"

print() {
    if [[ $# -ne 1 ]]; then
        echo "Wrong number of arguments! Expected 1 (msg)"
        return 1
    fi

    echo "> $1"
}

check_commands() {
    # Exit the loop
    if [[ "${1}" == "exit" ]]; then
        exit 0

    # Push to SE Script folder
    elif [[ "${1:0:4}" == "push" ]]; then
        if [[ -d "./Scripts/${1:5}" ]]; then
            cp -r "./Scripts/${1:5}" "./temp/"
            sed -i '/#region Prelude/, /YOUR CODE BEGIN/d' "./temp/script.cs"
            sed -i "/YOUR CODE END/g" "./temp/script.cs"
            rm -r "$SEScripts/${1:5}" 2> /dev/null
            mv ./temp "$SEScripts/${1:5}"
            rm -rf ./temp
            print "Done!"
        else
            print "Script doesn't exist! Push failed!"
        fi

    # Create new Script from Default
    elif [[ "${1:0:6}" == "create" ]]; then
        if [[ ! -d "./Scripts/${1:7}" ]] && [[ -n ${1:7} ]]; then
            TEMP=${1:7}
            cp -r default "./Scripts/$TEMP"
            CONTENT=$(cat ./default/script.cs)
            echo "${CONTENT/DEFAULT/${TEMP// /}}" > "./Scripts/$TEMP/script.cs"
            print "Done!"
        else
            print "Script exists or no script-name was given! Create failed!"
        fi

    # Remove Script
    elif [[ "${1:0:2}" == "rm" ]]; then
        if [[ -d "./Scripts/${1:3}" ]]; then
            rm -rf "./Scripts/${1:3}"
            print "Done!"
        else
            print "Script doesn't exist! Remove failed!"
        fi

    # Remove from SEScripts
    elif [[ "${1:0:4}" == "serm" ]]; then
        if [[ -d "$SEScripts/${1:5}" ]]; then
            rm -rf "$SEScripts/${1:5}"
            print "Done!"
        else
            print "Script doesn't exist! Remove failed!"
        fi

    # Pull Script from SE Folder
    elif [[ "${1:0:4}" == "pull" ]]; then
        if [[ -d "$SEScripts/${1:5}" ]]; then
            cp -r "$SEScripts/${1:5}" "./Scripts/"
            cp default/thumb.png "./Scripts/${1:5}/thumb.png"
            TEMP=$(cat "./Scripts/${1:5}/script.cs")
            cat extensions/header.txt > "./Scripts/${1:5}/script.cs"
            echo "$TEMP" >> "./Scripts/${1:5}/script.cs"
            cat extensions/footer.txt >> "./Scripts/${1:5}/script.cs"
            print "Done!"
        else
            print "Script does not exist! Pull failed!"
        fi

    # List Scripts
    elif [[ "$1" == "list" ]]; then
        for i in ./Scripts/*; do
            print "${i##*/}"
        done

    # List SE Scripts
    elif [[ "$1" == "selist" ]]; then
        for i in $SEScripts/*; do
            print "${i##*/}"
        done

    # Clear
    elif [[ "$1" == "clear" ]]; then
        clear

    # Help Page
    elif [[ "$1" == "help" ]]; then
        echo "---   HELP PAGE   ---
Command list:
list   -> Lists all available Scripts
selist -> Lists all available Scripts in the SpaceEngineers folder
create -> Creates a new script from default template
rm     -> Removes existing Script
serm   -> Removes existing Script from SpaceEngineers
pull   -> Pulls available Script from SpaceEngineers
push   -> Pushes Script to SapceEngineers
clear  -> Clears output
help   -> Shows this help page
exit   -> Exits the Program" | less

    # Unknwon command
    else
        print "Unknown command! Type 'help' for more info!"
    fi
}

if [[ -n $1 ]]; then
    check_commands "$*"
    exit 0
fi

while true; do
    echo -n $CLM
    check_commands "$(head -n1 /dev/stdin)"
done
