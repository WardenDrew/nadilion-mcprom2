dialog --default-button "no" \
	--no-label "Take me back" \
	--yes-label "I will remember" \
	--yesno "CAUTION! You are about to attach to a running docker container's TTY. In order to detach without breaking something you will need to use the key sequence CTRL-P CTRL-Q" 0 0
dialog_status=$?
clear

if [ "$dialog_status" -eq 0 ]; then
	echo "===== [[ SHOWING LAST 100 LOG LINES ]] ====="
	docker logs --tail 100 mc-prom2
	docker attach mc-prom2
fi


