import React, { Component } from "react";
import Container from "react-bootstrap/Container";
import Row from "react-bootstrap/Row";
import Col from "react-bootstrap/Col";
import Button from "react-bootstrap/Button";
import { CreateWebsiteRecordModalWindow } from "./CreateWebsiteRecordModalWindow";

export class Home extends Component {
  static displayName = Home.name;

  constructor(props) {
    super(props);

    this.state = {
      isCreateWebsiteRecordModalShown: false,
    };
  }

  render() {
    return (
      <>
        <Container fluid="md">
          <Row>{/*add the paginated view of website records here*/}</Row>
          <Row>
            <Col>
              <Button
                variant="primary"
                onClick={() =>
                  this.setState({ isCreateWebsiteRecordModalShown: true })
                }
              >
                Create new website record
              </Button>
            </Col>
          </Row>
        </Container>

        <CreateWebsiteRecordModalWindow
          show={this.state.isCreateWebsiteRecordModalShown}
          handleClose={() =>
            this.setState({ isCreateWebsiteRecordModalShown: false })
          }
        />
      </>
    );
  }
}
