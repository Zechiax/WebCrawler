import React, { Component } from "react";
import Container from "react-bootstrap/Container";
import Row from "react-bootstrap/Row";
import Modal from "react-bootstrap/Modal";
import Button from "react-bootstrap/Button";
import Form from "react-bootstrap/Form";
import InputGroup from "react-bootstrap/InputGroup";
import Alert from "react-bootstrap/Alert";

export class CreateWebsiteRecordModalWindow extends Component {
  constructor(props) {
    super(props);
    this.state = {
      validated: false,
      submitSuccess: false,
      submitBadRequest: false,
      submitOtherError: false,
      cantAddTagError: false,
      tags: [],
    };
  }

  handleSubmit = (event) => {
    event.preventDefault();
    const form = event.currentTarget;

    if (form.checkValidity() === false) {
      event.stopPropagation();
      this.setState({ validated: true });

      return;
    }

    const formData = Object.fromEntries(new FormData(form).entries());
    formData["tags"] = this.state.tags;
    console.log(formData);

    fetch("/Record", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(formData),
    })
      .then((response) => {
        console.log(response.status);
        if (response.status === 200) {
          this.setState({ submitSuccess: true });
        } else {
          this.setState({ submitBadRequest: true });
        }
      })
      .catch((error) => this.setState({ submitOtherError: true }));

    this.setState({ validated: false });

    this.resetForm(form);
  };

  resetForm = (form) => {
    form.reset();
    this.setState({ validated: false, tags: [] });
  };

  render() {
    return (
      <>
        <Modal
          show={this.props.show}
          onHide={this.props.onClose}
          size="xl"
          centered
        >
          <Modal.Header closeButton>
            <Container>
              <Row>
                <Alert
                  dismissible
                  show={this.state.submitSuccess}
                  onClose={() => this.setState({ submitSuccess: false })}
                  variant="success"
                >
                  Website record successfully added for crawling!
                </Alert>

                <Alert
                  dismissible
                  show={this.state.submitBadRequest}
                  onClose={() => this.setState({ submitBadRequest: false })}
                  variant="warning"
                >
                  Can't create website record with specified data!
                </Alert>

                <Alert
                  dismissible
                  show={this.state.submitOtherError}
                  onClose={() => this.setState({ submitOtherError: false })}
                  variant="danger"
                >
                  Oops! Something went wrong. Try again a little later.
                </Alert>

                <Alert
                  dismissible
                  show={this.state.cantAddTagError}
                  onClose={() => this.setState({ cantAddTagError: false })}
                  variant="warning"
                >
                  Can't add this tag. It is either already included, too long,
                  empty or too many tags are already assigned.
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
              validated={this.state.validated}
              onSubmit={this.handleSubmit}
            >
              <Form.Group>
                <Form.Label>Label</Form.Label>
                <Form.Control
                  id="label"
                  name="label"
                  type="text"
                  required
                  placeholder="Label"
                  defaultValue="Wikipedia"
                />
                <Form.Control.Feedback>Looks good!</Form.Control.Feedback>
                <Form.Control.Feedback type="invalid">
                  Is required
                </Form.Control.Feedback>
                <Form.Text id="label help" muted>
                  With this label you will be able to easily identify this
                  website record.
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
                  defaultValue="https://en.wikipedia.org/wiki/Main_Page"
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
                  defaultValue=".*"
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
                <Form.Label>Periodicity</Form.Label>
                <Form.Control
                  id="periodicity"
                  name="periodicity"
                  required
                  type="number"
                  placeholder="Periodicity in minutes"
                  defaultValue="10"
                />
                <Form.Text id="periodicity help" muted>
                  In minutes. The page will be crawled periodically counting
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
                      const tagInput = document.getElementById("add-tag-input");
                      const value = tagInput.value;

                      if (
                        !value ||
                        value.length > 30 ||
                        this.state.tags.length >= 50 ||
                        this.state.tags.includes(value)
                      ) {
                        this.setState({ cantAddTagError: true });
                        return;
                      }

                      this.setState((state, props) => ({
                        tags: [...state.tags, value],
                      }));

                      tagInput.value = "";
                    }}
                  >
                    Add
                  </Button>
                </InputGroup>
                <Form.Text>
                  Tags can help to organize and filter website records. Is
                  optional.
                </Form.Text>
                <ul
                  className="list-group list-group-vertical"
                  style={{ fontSize: 14 }}
                >
                  {this.state.tags.map((tag) => {
                    return <li className="list-group-item flex-fill">{tag}</li>;
                  })}
                </ul>
              </Form.Group>

              <Form.Group>
                <Form.Check
                  id="isActive"
                  name="isActive"
                  type="checkbox"
                  label="Is active"
                  defaultChecked
                />
                <Form.Text id="isActive help" muted>
                  No crawling at all is performed if inactive.
                </Form.Text>
              </Form.Group>
            </Form>
          </Modal.Body>
          <Modal.Footer>
            <Button variant="secondary" onClick={this.props.onClose}>
              Close
            </Button>
            <Button type="submit" form="form" variant="success">
              Create
            </Button>
          </Modal.Footer>
        </Modal>
      </>
    );
  }
}
