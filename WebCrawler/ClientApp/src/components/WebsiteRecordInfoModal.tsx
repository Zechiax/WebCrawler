import React, { useState, useEffect } from "react";
import { Container, Row, Alert, Modal, Button, Form, InputGroup, Col } from "react-bootstrap";
import { Delete } from "@mui/icons-material";
import Chip from "@mui/material/Chip";

interface Props {
    show: boolean;
    onClose: () => void;
    isEditing: boolean;
    recordId: string;
    tagsPresetValue: { name: string }[];
    passCreatedRecordId?: (id: string) => void;
    labelPresetValue?: string;
    urlPresetValue?: string;
    periodicityPresetValue?: number;
    regexPresetValue?: string;
    isActivePresetValue?: boolean;
}

const WebsiteRecordInfoModal: React.FC<Props> = ({
    show,
    onClose,
    isEditing,
    recordId,
    tagsPresetValue,
    passCreatedRecordId,
    labelPresetValue,
    urlPresetValue,
    periodicityPresetValue,
    regexPresetValue,
    isActivePresetValue,
}) => {
    const [validated, setValidated] = useState(false);
    const [submitSuccess, setSubmitSuccess] = useState(false);
    const [submitBadRequest, setSubmitBadRequest] = useState(false);
    const [submitOtherError, setSubmitOtherError] = useState(false);
    const [cantAddTagError, setCantAddTagError] = useState(false);
    const [tags, setTags] = useState<string[]>([]);
    const [periodicityMinutes, setPeriodicityMinutes] = useState('');
    const [periodicitySeconds, setPeriodicitySeconds] = useState('');
    const [periodicityHours, setPeriodicityHours] = useState('');

    useEffect(() => {
        const updateTagsState = () => {
            const newTags = tagsPresetValue ? tagsPresetValue.map((tag) => tag.name) : [];
            setTags(newTags);
        };

        const updatePeriodicityStates = () => {
            const hours = Math.floor(periodicityPresetValue / 3600);
            const minutes = Math.floor((periodicityPresetValue - (hours * 3600)) / 60);
            const seconds = periodicityPresetValue - (hours * 3600) - (minutes * 60);
            setPeriodicityHours(hours.toString());
            setPeriodicityMinutes(minutes.toString());
            setPeriodicitySeconds(seconds.toString());
        }

        if (tagsPresetValue) {
            updateTagsState();
        }

        if (periodicityPresetValue) {
            updatePeriodicityStates();
        }

    }, [tagsPresetValue, periodicityPresetValue]);

    const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        const form = event.currentTarget;

        if (form.checkValidity() === false) {
            event.stopPropagation();
            setValidated(true);
            return;
        }

        const { periodicityMinutes, periodicitySeconds, periodicityHours, ...formDataValues } = Object.fromEntries(new FormData(form).entries());
        const totalSeconds = (Number(periodicityHours) * 3600) + (Number(periodicityMinutes) * 60) + Number(periodicitySeconds);
        const requestData = {
            ...formDataValues,
            tags: tags,
            isActive: (form.elements.namedItem("isActive") as HTMLInputElement).checked,
            periodicity: totalSeconds.toString()
        };

        console.log(requestData);

        try {
            let url = "/record";
            let method = "POST";

            if (isEditing) {
                url = `/record/${recordId}`;
                method = "PATCH";
            }

            const response = await fetch(url, {
                method: method,
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(requestData),
            });

            console.log("response status: " + response.status);
            if (response.status === 200) {
                setSubmitSuccess(true);
            } else if (response.status === 500) {
                setSubmitOtherError(true);
            } else {
                setSubmitBadRequest(true);
            }

            try {
                const id = await response.json();
                if (passCreatedRecordId !== undefined) {
                    passCreatedRecordId(id);
                }
            } catch {
                console.log("Server returning id from server failed when created a new website record.");
            }
        } catch {
            setSubmitOtherError(true);
        }

        setValidated(false);
        resetForm(form);
    };

    const resetForm = (form: HTMLFormElement) => {
        form.reset();
        setValidated(false);
        setTags([]);
    };

    const handleDeleteTag = (index: number, event: React.MouseEvent<HTMLButtonElement>) => {
        event.preventDefault();
        setTags((prevTags) => {
            const newTags = [...prevTags];
            newTags.splice(index, 1);
            return newTags;
        });
    };

    const buttonLabel = isEditing ? "Save" : "Create";

    return (
        <>
            <Modal
                show={show}
                onHide={() => {
                    setSubmitSuccess(false);
                    setSubmitBadRequest(false);
                    setSubmitOtherError(false);
                    onClose();
                }}
                size="xl"
                centered
            >
                <Modal.Header closeButton>
                    <Container>
                        <Row>
                            <Alert
                                dismissible
                                show={submitSuccess}
                                onClose={() => setSubmitSuccess(false)}
                                variant="success"
                            >
                                Website record successfully added for crawling!
                            </Alert>

                            <Alert
                                dismissible
                                show={submitBadRequest}
                                onClose={() => setSubmitBadRequest(false)}
                                variant="warning"
                            >
                                Can't create website record with specified data!
                            </Alert>

                            <Alert
                                dismissible
                                show={submitOtherError}
                                onClose={() => setSubmitOtherError(false)}
                                variant="danger"
                            >
                                Oops! Something went wrong. Try again a little later.
                            </Alert>

                            <Alert
                                dismissible
                                show={cantAddTagError}
                                onClose={() => setCantAddTagError(false)}
                                variant="warning"
                            >
                                Can't add this tag. It is either already included, too long, empty, or too many tags are already assigned.
                            </Alert>
                        </Row>
                        <Row>
                            <Modal.Title>Create Website Record</Modal.Title>
                        </Row>
                    </Container>
                </Modal.Header>
                <Modal.Body>
                    <Form
                        id="form"
                        noValidate
                        validated={validated}
                        onSubmit={handleSubmit}
                    >
                        <Form.Group>
                            <Form.Label>Label</Form.Label>
                            <Form.Control
                                id="label"
                                name="label"
                                type="text"
                                required
                                placeholder="Label"
                                defaultValue={labelPresetValue}
                            />
                            <Form.Control.Feedback>Looks good!</Form.Control.Feedback>
                            <Form.Control.Feedback type="invalid">
                                Is required
                            </Form.Control.Feedback>
                            <Form.Text id="label help" muted>
                                With this label, you will be able to easily identify this website record.
                            </Form.Text>
                        </Form.Group>

                        <Form.Group>
                            <Form.Label>Entry Url</Form.Label>
                            <Form.Control
                                id="url"
                                name="url"
                                type="text"
                                required
                                placeholder="Entry Url"
                                defaultValue={ urlPresetValue }
                            />
                            <Form.Control.Feedback>Looks good!</Form.Control.Feedback>
                            <Form.Control.Feedback type="invalid">
                                Is required
                            </Form.Control.Feedback>
                            <Form.Text id="entry url help" muted>
                                Entry url from which the crawling of this website record will
                                begin.
                            </Form.Text>
                        </Form.Group>

                        <Form.Group>
                            <Form.Label>Regex</Form.Label>
                            <Form.Control
                                id="regex"
                                name="regex"
                                type="text"
                                required
                                placeholder="Regex"
                                defaultValue={ regexPresetValue }
                            />
                            <Form.Control.Feedback>Looks good!</Form.Control.Feedback>
                            <Form.Control.Feedback type="invalid">
                                Is required
                            </Form.Control.Feedback>
                            <Form.Text id="regex help" muted>
                                Only found urls matching this regex will be crawled further.
                                Otherwise, they are just ignored.
                            </Form.Text>
                        </Form.Group>

                        <Form.Group>
                            <Form.Label column sm={2}>Periodicity</Form.Label>
                            <Col sm={10} className="d-flex align-items-center">
                                <div className="d-flex align-items-center">
                                    h:
                                    <Form.Control
                                        id="periodicity-hours"
                                        name="periodicityHours"
                                        required
                                        type="number"
                                        placeholder="Hours"
                                        value={periodicityHours}
                                        onChange={(e) => setPeriodicityHours(e.target.value)}
                                        style={{
                                            marginRight: "0.5rem",
                                            marginLeft: "0.2rem"
                                        }}
                                    />
                                    m:
                                    <Form.Control
                                        className="mr-2"
                                        id="periodicity-minutes"
                                        name="periodicityMinutes"
                                        required
                                        type="number"
                                        placeholder="Minutes"
                                        value={periodicityMinutes}
                                        onChange={(e) => setPeriodicityMinutes(e.target.value)}
                                        style={{
                                            marginRight: "0.5rem",
                                            marginLeft: "0.2rem"
                                        }}
                                    />
                                    s:
                                    <Form.Control
                                        className="mr-2"
                                        id="periodicity-seconds"
                                        name="periodicitySeconds"
                                        required
                                        type="number"
                                        placeholder="Seconds"
                                        value={periodicitySeconds}
                                        onChange={(e) => setPeriodicitySeconds(e.target.value)}
                                        style={{
                                            marginRight: "50%",
                                            marginLeft: "0.2rem"
                                        }}
                                    />
                                </div>
                            </Col>
                            <Form.Text id="periodicity help" muted>
                                The page will be crawled periodically counting
                                from start of the last crawl. For the first time, it is added
                                for crawl instantly.
                            </Form.Text>
                        </Form.Group>

                        <Form.Group>
                            <Form.Label>Tags</Form.Label>
                            <InputGroup className="mb-3">
                                <Form.Control
                                    placeholder="write some tag"
                                    aria-label="write some tag"
                                    id="add-tag-input"
                                />
                                <Button
                                    variant="outline-secondary"
                                    aria-describedby="add-tag-input"
                                    onClick={() => {
                                        const tagInput = document.getElementById("add-tag-input") as HTMLInputElement;
                                        const value = tagInput.value;

                                        if (!value || value.length > 30 || tags.length >= 50 || tags.includes(value)) {
                                            setCantAddTagError(true);
                                            return;
                                        }

                                        setTags((prevTags) => [...prevTags, value]);

                                        tagInput.value = "";
                                    }}
                                >
                                    Add
                                </Button>
                            </InputGroup>
                            <Form.Text>
                                Tags can help to organize and filter website records. They are optional.
                            </Form.Text>
                            <ul className="list-group list-group-vertical" style={{ fontSize: 14 }}>
                                {tags.map((tag, index) => (
                                    <li className="list-group-item flex-fill d-flex justify-content-between" key={index}>
                                        <Chip
                                            label={tag}
                                            variant="outlined"
                                            sx={{
                                                margin: "0.2rem",
                                                color: "purple",
                                                borderColor: "purple",
                                                fontWeight: "bold",
                                            }}
                                        />
                                        <span className="float-right">
                                            <button
                                                className="btn btn-sm btn-outline-danger ml-2"
                                                onClick={(event) => handleDeleteTag(index, event)}
                                            >
                                                <Delete />
                                            </button>
                                        </span>
                                    </li>
                                ))}
                            </ul>
                        </Form.Group>
                        <Form.Group>
                            <Form.Check
                                id="isActive"
                                name="isActive"
                                type="checkbox"
                                label="Is active"
                                defaultChecked={isActivePresetValue}
                            />
                            <Form.Text id="isActive help" muted>
                                No crawling at all is performed if inactive.
                            </Form.Text>
                        </Form.Group>
                    </Form>

                </Modal.Body>
                <Modal.Footer>
                    <Button variant="secondary" onClick={onClose}>
                        Close
                    </Button>
                    <Button type="submit" form="form" variant="success">
                        {buttonLabel}
                    </Button>
                </Modal.Footer>
            </Modal>
        </>
    );
};

export default WebsiteRecordInfoModal;
