rsync -rav -e ssh \
    --exclude="vc_state/*" \
    --exclude="msg_log/*" \
    --exclude="log.txt" \
    --exclude="call_stats/*" \
    --exclude="casino/user_data/*" \
    --exclude="live_stats/*" \
    --exclude="deleted/*" \
    ~/.config/alkoholiker/ \
    cinneyyy@192.168.2.100:/home/cinneyyy/.config/alkoholiker
