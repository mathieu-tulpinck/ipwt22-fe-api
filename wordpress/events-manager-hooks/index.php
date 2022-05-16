<?php

/**
 * Plugin Name: Events manager hooks
 */

define("PRODUCER_URL", "http://frontend-producer-1:5000/");

add_filter('em_event_save', 'notify_event', 10, 2);

function notify_event($result, $event) {
    $post = get_post($event->post_id);
    $created  = strtotime($post->post_date_gmt);
    $modified = strtotime($post->post_modified_gmt);
    $event_start = strtotime($event->event_start_date . " " . $event->event_start_time);
    $event_end = strtotime($event->event_start_date . " " . $event->event_start_time);
    // Created
    if ($created == $modified && $event->event_status == 1) {
        $status = true;
        $method = 'CREATE';
        send_event_payload($event, $event_start, $event_end, $status, $method);
    } 
    // Place in draft
    else if (is_null($event->event_status)) {
        $status = false;
        $method = 'UPDATE';
        send_event_payload($event, $event_start, $event_end, $status, $method);
    }

    return $result;
}

add_action('em_event_save_pre', 'notify_event_update');

function notify_event_update($event) {
    $post = get_post($event->post_id);
    $created  = strtotime($post->post_date_gmt);
    $modified = strtotime($post->post_modified_gmt);
    $event_start = strtotime($event->event_start_date . " " . $event->event_start_time);
    $event_end = strtotime($event->event_start_date . " " . $event->event_start_time);
    
    // Update (relevant attributes only)
    if ($created != $modified && $event->event_status == 1) {
        $event_id = $event->event_id;
        global $wpdb;
        $old = $wpdb->get_row($wpdb->prepare("SELECT * FROM " . EM_EVENTS_TABLE . ' WHERE event_id=%s', $event_id), ARRAY_A);
        $master = array('event_id' => 0, 'event_owner' => 0, 'event_status' => 0, 'event_name' => 0, 'event_start' => 0, 'event_end' => 0);
        $old_intersect = array_intersect_key($old, $master);
        $old_intersect["event_start"] = strtotime($old_intersect["event_start"]);
        $old_intersect["event_end"] = strtotime($old_intersect["event_end"]);
        $event_array = json_decode(json_encode($event), true);
        $event_array_intersect = array_intersect_key($event_array, $master);
        $event_array_intersect["event_start"] = strtotime($event->event_start_date." ".$event->event_start_time);
        $event_array_intersect["event_end"] = strtotime($event->event_end_date." ".$event->event_end_time);
        $diff = array_diff($old_intersect, $event_array_intersect);
        if (!empty(array_diff($old_intersect, $event_array_intersect))) {
            $status = true;
            $method = 'UPDATE';
            send_event_payload($event, $event_start, $event_end, $status, $method);
        }
    } 
}

add_action('wp_trash_post', 'notify_event_delete');

function notify_event_delete($post_id) {
    global $wpdb;
    $event = $wpdb->get_row($wpdb->prepare("SELECT * FROM " . EM_EVENTS_TABLE . ' WHERE post_id=%s', $post_id), OBJECT);
    $event_start = strtotime($event->event_start);
    $event_end = strtotime($event->event_end);
    $status = false;
    $method = 'DELETE';
    send_event_payload($event, $event_start, $event_end, $status, $method);
}

function send_event_payload($event, $event_start, $event_end, $status, $method) {
    $headers = array(
        'Content-Type' => 'application/json'
    );
    
    $body = array(
        'id' => $event->event_id,
        'owner' => $event->event_owner, // OrganiserUuid
        'status' => $status, // IsActive
        'name' => $event->event_name, // Title
        'start' => $event_start, // StartDateUTC
        'end' => $event_end, // EndDateUTC
        'method' => $method
    );

    $args = array(
        'headers' => $headers,     
        'body' => $body
    );
    wp_remote_post("{$PRODUCER_URL}/api/events", $args);
}

// 0 => 'Pending',
// 1 => Approved',
// 2 => 'Rejected',
// 3 => 'Cancelled',
// 4 => 'Awaiting Online Payment',
// 5 => 'Awaiting Payment'
add_filter('em_booking_save', 'notify_booking_created', 10, 3);

function notify_booking_created ($count, $booking, $update) {
    $method = 'CREATE';
    send_booking_payload($booking, $method);

    return $count;
}

add_filter('em_booking_set_status','notify_booking_status', 10, 2);

function notify_booking_status($result, $booking) {
    $method = 'UPDATE';
    send_booking_payload($booking, $method);

    return $result;
}

add_filter('em_booking_delete', 'notify_booking_deleted', 10, 2);

function notify_booking_deleted ($result, $booking) {
    $method = 'DELETE';
    send_booking_payload($booking, $method);

    return $result;
}

function send_booking_payload($booking, $method) {
    $headers = array(
        'Content-Type' => 'application/json'
    );
    
    $body = array(
        'id' => $booking->booking_id,
        'eventId' => $booking->event_id,
        'personId' => $booking->person_id, // Name, LastName, Email, VatNumber
        'bookingSpaces' => $booking->booking_spaces,
        'bookingStatus' => $booking->booking_status, // InvitationStatus
        'method' => $method // Method
    );

    $args = array(
        'headers' => $headers,     
        'body' => $body
    );
    wp_remote_post("{$PRODUCER_URL}/api/bookings", $args);
}

